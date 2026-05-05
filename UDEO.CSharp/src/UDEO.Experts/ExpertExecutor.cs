using System.Diagnostics;
using UDEO.Core.Logging;
using UDEO.Core.Models;

namespace UDEO.Experts;

/// <summary>
/// Executes an expert by ID with the given context and parameters.
/// Replaces the PowerShell Invoke-UDEOExpert function.
/// </summary>
public sealed class ExpertExecutor
{
    private static readonly Lazy<ExpertExecutor> _instance = new(() => new ExpertExecutor());
    public static ExpertExecutor Instance => _instance.Value;

    private ExpertExecutor() { }

    /// <summary>
    /// Execute a registered expert with timeout enforcement.
    /// </summary>
    public ExpertResult Execute(
        string expertId,
        ExecutionContext context,
        Dictionary<string, object?>? parameters = null)
    {
        parameters ??= new Dictionary<string, object?>();

        var contract = ExpertRegistry.Instance.Get(expertId);
        if (contract == null)
        {
            var trace = new DecisionTrace(expertId, "Unknown", "EXPERT_NOT_FOUND", "ERROR", 0);
            context.RecordDecision(trace);
            return ExpertResult.FailureResult($"Expert not found: {expertId}", context);
        }

        UdeoLogger.Instance.Debug($"Executing expert: {contract.Name} [{expertId}]");
        var sw = Stopwatch.StartNew();

        try
        {
            ExpertResult result;

            if (contract.TimeoutSeconds > 0)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(contract.TimeoutSeconds));
                var task = Task.Run(() => contract.Execute(context, parameters), cts.Token);

                if (task.Wait(TimeSpan.FromSeconds(contract.TimeoutSeconds + 1)))
                    result = task.Result;
                else
                    throw new TimeoutException($"Expert {expertId} timed out after {contract.TimeoutSeconds}s");
            }
            else
            {
                result = contract.Execute(context, parameters);
            }

            sw.Stop();
            var elapsed = Math.Round(sw.Elapsed.TotalMilliseconds, 2);

            var code = result.Success ? (result.DecisionCode ?? "PENDING") : "ERROR";
            var rule = !string.IsNullOrEmpty(result.RuleFired) ? result.RuleFired :
                       result.Success ? "EXECUTED" : (result.Error ?? "EXECUTION_FAILED");

            var trace = new DecisionTrace(expertId, contract.Name, rule, code, elapsed);
            context.RecordDecision(trace);

            UdeoLogger.Instance.Info($"Expert {expertId} -> {code} ({elapsed}ms)", new { rule });

            if (result.Context != null)
                context = result.Context;

            return new ExpertResult
            {
                Success = result.Success,
                DecisionCode = code,
                RuleFired = rule,
                Error = result.Error,
                Context = context,
                ExecutionTimeMs = elapsed
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            var elapsed = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
            var errorMsg = ex is TimeoutException ? $"Expert timed out: {expertId}" : ex.Message;

            var trace = new DecisionTrace(expertId, contract.Name, errorMsg, "ERROR", elapsed);
            context.RecordDecision(trace);
            UdeoLogger.Instance.Error($"Expert {expertId} threw: {ex.Message}");

            return new ExpertResult
            {
                Success = false,
                DecisionCode = "ERROR",
                Error = ex.Message,
                Context = context,
                ExecutionTimeMs = elapsed
            };
        }
    }

    /// <summary>
    /// Execute an expert asynchronously.
    /// </summary>
    public async Task<ExpertResult> ExecuteAsync(
        string expertId,
        ExecutionContext context,
        Dictionary<string, object?>? parameters = null)
    {
        return await Task.Run(() => Execute(expertId, context, parameters));
    }
}

using System.Diagnostics;
using UDEO.Core;
using UDEO.Core.Logging;
using UDEO.Core.Models;
using UDEO.Experts;

namespace UDEO.Pipeline;

/// <summary>
/// Expert pipeline orchestrator. Runs a sequence of expert steps with
/// conditional skipping, terminal decisions, failure policies, and retries.
/// Replaces the PowerShell UDEOPipeline class.
/// </summary>
public sealed class UdeoPipeline
{
    private static readonly HashSet<string> TerminalDecisions = new(StringComparer.OrdinalIgnoreCase)
    {
        "APPROVED", "REJECTED", "FLAGGED"
    };

    public string Id { get; }
    public string Name { get; }
    public UdeoPipelineStatus Status { get; private set; } = UdeoPipelineStatus.Pending;
    public ExecutionContext Context { get; set; }
    public IReadOnlyList<PipelineStep> Steps => _steps.AsReadOnly();

    private readonly List<PipelineStep> _steps = new();
    private DateTime _startedAt;
    private DateTime _completedAt;

    public UdeoPipeline(string name)
    {
        Id = $"pipeline_{Guid.NewGuid().ToString()[..8]}";
        Name = name;
        Context = new ExecutionContext(Id);
    }

    public UdeoPipeline(string name, string id, ExecutionContext context)
    {
        Id = id;
        Name = name;
        Context = context;
    }

    /// <summary>
    /// Adds an unconditional step to the pipeline.
    /// </summary>
    public void AddStep(
        string expertId,
        Dictionary<string, object?>? parameters = null,
        FailurePolicy onFailure = FailurePolicy.Stop,
        int maxRetries = 0,
        TimeSpan? timeout = null)
    {
        parameters ??= new Dictionary<string, object?>();
        parameters["OnFailure"] = onFailure.ToString().ToLowerInvariant();
        _steps.Add(new PipelineStep(expertId, parameters, null, onFailure, maxRetries, timeout));
    }

    /// <summary>
    /// Adds a conditional step that only runs when the condition returns false (matches PS behavior).
    /// In PowerShell, condition returning true means "skip". We replicate by negating.
    /// </summary>
    public void AddConditionalStep(
        string expertId,
        Dictionary<string, object?>? parameters = null,
        Func<ExecutionContext, bool>? condition = null,
        FailurePolicy onFailure = FailurePolicy.Stop,
        int maxRetries = 0)
    {
        if (condition == null) { AddStep(expertId, parameters, onFailure, maxRetries); return; }

        parameters ??= new Dictionary<string, object?>();
        parameters["OnFailure"] = onFailure.ToString().ToLowerInvariant();
        // PowerShell condition returns $true to SKIP, so negate for C# semantics
        _steps.Add(new PipelineStep(expertId, parameters, ctx => !condition(ctx), onFailure, maxRetries));
    }

    /// <summary>
    /// Executes all pipeline steps sequentially.
    /// </summary>
    public PipelineResult Run()
    {
        Status = UdeoPipelineStatus.Running;
        _startedAt = DateTime.UtcNow;
        UdeoLogger.Instance.Info($"Pipeline started: {Name} [{Id}]", new { steps = _steps.Count });

        for (int i = 0; i < _steps.Count; i++)
        {
            var step = _steps[i];
            UdeoLogger.Instance.Info($"  Step {i + 1}/{_steps.Count}: {step.ExpertId}");
            Context.Data["_current_step"] = i + 1;
            Context.Data["_total_steps"] = _steps.Count;

            // Check condition — if condition returns true, skip
            if (step.Condition != null && step.Condition(Context))
            {
                UdeoLogger.Instance.Debug("  Skipping step (condition not met)");
                continue;
            }

            // Execute with retries
            ExpertResult? stepResult = null;
            Exception? lastException = null;

            for (int attempt = 0; attempt <= step.MaxRetries; attempt++)
            {
                try
                {
                    stepResult = ExpertExecutor.Instance.Execute(step.ExpertId, Context, step.Parameters);
                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (attempt < step.MaxRetries)
                    {
                        UdeoLogger.Instance.Warn($"  Step {i + 1} attempt {attempt + 1} failed. Retrying...");
                        Thread.Sleep(100 * (attempt + 1)); // Exponential-ish backoff
                    }
                }
            }

            if (stepResult == null && lastException != null)
            {
                stepResult = ExpertResult.FailureResult(lastException.Message, Context);
            }

            if (!stepResult!.Success)
            {
                var onFailure = step.OnFailure;
                if (step.Parameters.TryGetValue("OnFailure", out var rawPolicy))
                {
                    Enum.TryParse<FailurePolicy>(rawPolicy?.ToString(), true, out onFailure);
                }

                UdeoLogger.Instance.Warn($"  Step {i + 1} failed. OnFailure={onFailure}");

                switch (onFailure)
                {
                    case FailurePolicy.Stop:
                        Status = UdeoPipelineStatus.Failed;
                        _completedAt = DateTime.UtcNow;
                        return new PipelineResult
                        {
                            Success = false,
                            Decision = "ERROR",
                            Error = stepResult.Error ?? $"Step failed: {step.ExpertId}",
                            FailedStep = step.ExpertId,
                            FailedIndex = i + 1,
                            Trace = Context.GetTrace(),
                            DurationMs = (_completedAt - _startedAt).TotalMilliseconds
                        };

                    case FailurePolicy.Skip:
                        continue;

                    case FailurePolicy.Continue:
                        // Continue to next step
                        break;
                }
            }

            // Check for terminal decisions
            if (stepResult != null && TerminalDecisions.Contains(stepResult.DecisionCode))
            {
                UdeoLogger.Instance.Info($"  Terminal decision reached: {stepResult.DecisionCode}");
                Status = UdeoPipelineStatus.Completed;
                _completedAt = DateTime.UtcNow;
                return new PipelineResult
                {
                    Success = true,
                    Decision = stepResult.DecisionCode,
                    Reason = stepResult.RuleFired,
                    Trace = Context.GetTrace(),
                    Context = Context,
                    DurationMs = (_completedAt - _startedAt).TotalMilliseconds
                };
            }
        }

        // All steps completed without terminal
        Status = UdeoPipelineStatus.Completed;
        _completedAt = DateTime.UtcNow;

        var finalDecision = Context.Data.TryGetValue("_final_decision", out var fd)
            ? fd?.ToString() ?? "PENDING"
            : "PENDING";

        var result = new PipelineResult
        {
            Success = true,
            Decision = finalDecision,
            Trace = Context.GetTrace(),
            Context = Context,
            DurationMs = (_completedAt - _startedAt).TotalMilliseconds
        };

        UdeoLogger.Instance.Info($"Pipeline completed: {Name}", new
        {
            decision = result.Decision,
            steps = _steps.Count,
            duration = result.DurationMs
        });

        return result;
    }

    /// <summary>
    /// Executes the pipeline asynchronously.
    /// </summary>
    public async Task<PipelineResult> RunAsync()
        => await Task.Run(Run);

    /// <summary>
    /// Resets the pipeline for re-execution with fresh context.
    /// </summary>
    public void Reset()
    {
        var newId = $"pipeline_{Guid.NewGuid().ToString()[..8]}";
        Context = new ExecutionContext(newId);
        Status = UdeoPipelineStatus.Pending;
    }
}

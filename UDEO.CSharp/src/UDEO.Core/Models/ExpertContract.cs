using System.Text.Json.Serialization;

namespace UDEO.Core.Models;

/// <summary>
/// Contract that defines an expert's metadata and execution behavior.
/// </summary>
public sealed class ExpertContract
{
    public string Id { get; }
    public string Name { get; }
    public string Version { get; }
    public UdeoExpertType Type { get; }
    public string Description { get; set; }
    public int TimeoutSeconds { get; set; }
    public Func<ExecutionContext, Dictionary<string, object?>, ExpertResult> Execute { get; }
    public Func<bool> HealthCheck { get; set; }

    public ExpertContract(
        string id,
        string name,
        UdeoExpertType type,
        Func<ExecutionContext, Dictionary<string, object?>, ExpertResult> execute)
    {
        Id = id;
        Name = name;
        Version = "3.1.0";
        Type = type;
        Description = string.Empty;
        TimeoutSeconds = 30;
        Execute = execute;
        HealthCheck = () => true;
    }

    /// <summary>
    /// Runs the health check for this expert.
    /// </summary>
    public bool IsHealthy()
    {
        try
        {
            return HealthCheck();
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Result returned by an expert execution.
/// </summary>
public sealed class ExpertResult
{
    public bool Success { get; init; }
    public string DecisionCode { get; init; } = "PENDING";
    public string RuleFired { get; init; } = string.Empty;
    public string? Error { get; init; }
    public ExecutionContext? Context { get; init; }
    public double ExecutionTimeMs { get; init; }

    public UdeoDecisionCode GetDecisionCodeEnum() =>
        Enum.TryParse<UdeoDecisionCode>(DecisionCode, true, out var code)
            ? code
            : UdeoDecisionCode.Pending;

    public static ExpertResult SuccessResult(string decisionCode, string ruleFired, ExecutionContext? context = null, double executionTimeMs = 0)
        => new()
        {
            Success = true,
            DecisionCode = decisionCode,
            RuleFired = ruleFired,
            Context = context,
            ExecutionTimeMs = executionTimeMs
        };

    public static ExpertResult FailureResult(string error, ExecutionContext? context = null, double executionTimeMs = 0)
        => new()
        {
            Success = false,
            DecisionCode = "ERROR",
            RuleFired = error,
            Error = error,
            Context = context,
            ExecutionTimeMs = executionTimeMs
        };
}

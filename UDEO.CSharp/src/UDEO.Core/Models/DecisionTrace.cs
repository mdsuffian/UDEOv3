using System.Text.Json.Serialization;

namespace UDEO.Core.Models;

/// <summary>
/// Immutable record of a single expert decision within a pipeline execution.
/// </summary>
public sealed record DecisionTrace
{
    public string ExpertId { get; init; }
    public string ExpertName { get; init; }
    public string RuleFired { get; init; }
    public string DecisionCode { get; init; }
    public DateTime Timestamp { get; init; }
    public double ExecutionTimeMs { get; init; }
    public Dictionary<string, object?> Metadata { get; init; }

    public DecisionTrace(
        string expertId,
        string expertName,
        string ruleFired,
        string decisionCode,
        double executionTimeMs)
    {
        ExpertId = expertId;
        ExpertName = expertName;
        RuleFired = ruleFired;
        DecisionCode = decisionCode;
        Timestamp = DateTime.UtcNow;
        ExecutionTimeMs = executionTimeMs;
        Metadata = new Dictionary<string, object?>();
    }

    [JsonConstructor]
    public DecisionTrace(
        string expertId,
        string expertName,
        string ruleFired,
        string decisionCode,
        DateTime timestamp,
        double executionTimeMs,
        Dictionary<string, object?> metadata)
    {
        ExpertId = expertId;
        ExpertName = expertName;
        RuleFired = ruleFired;
        DecisionCode = decisionCode;
        Timestamp = timestamp;
        ExecutionTimeMs = executionTimeMs;
        Metadata = metadata ?? new Dictionary<string, object?>();
    }

    public UdeoDecisionCode GetDecisionCodeEnum() =>
        Enum.TryParse<UdeoDecisionCode>(DecisionCode, true, out var code)
            ? code
            : UdeoDecisionCode.Pending;
}

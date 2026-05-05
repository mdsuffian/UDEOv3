using UDEO.Core;
using UDEO.Core.Models;

namespace UDEO.Pipeline;

/// <summary>
/// A single step in a pipeline, composed of an expert reference, parameters,
/// and optional condition/retry/failure policy.
/// </summary>
public sealed class PipelineStep
{
    public string ExpertId { get; }
    public Dictionary<string, object?> Parameters { get; }
    public Func<ExecutionContext, bool>? Condition { get; }
    public FailurePolicy OnFailure { get; }
    public int MaxRetries { get; }
    public TimeSpan? Timeout { get; }

    public PipelineStep(
        string expertId,
        Dictionary<string, object?>? parameters = null,
        Func<ExecutionContext, bool>? condition = null,
        FailurePolicy onFailure = FailurePolicy.Stop,
        int maxRetries = 0,
        TimeSpan? timeout = null)
    {
        ExpertId = expertId;
        Parameters = parameters ?? new Dictionary<string, object?>();
        Condition = condition;
        OnFailure = onFailure;
        MaxRetries = Math.Max(0, maxRetries);
        Timeout = timeout;
    }
}

/// <summary>
/// Pipeline failure handling policy.
/// </summary>
public enum FailurePolicy
{
    /// <summary>Stop the pipeline immediately on failure.</summary>
    Stop,
    /// <summary>Continue to next step despite failure.</summary>
    Continue,
    /// <summary>Skip the failed step and proceed.</summary>
    Skip
}

/// <summary>
/// Result of a complete pipeline execution.
/// </summary>
public sealed class PipelineResult
{
    public bool Success { get; init; }
    public string Decision { get; init; } = "PENDING";
    public string? Reason { get; init; }
    public string? Error { get; init; }
    public string? FailedStep { get; init; }
    public int? FailedIndex { get; init; }
    public IReadOnlyList<DecisionTrace> Trace { get; init; } = Array.Empty<DecisionTrace>();
    public ExecutionContext? Context { get; init; }
    public double DurationMs { get; init; }
}

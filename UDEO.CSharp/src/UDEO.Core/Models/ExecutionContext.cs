using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace UDEO.Core.Models;

/// <summary>
/// Carries all state through a pipeline execution: data, decision trace, and metadata.
/// Replaces the PowerShell UDEOContext class.
/// </summary>
public sealed class ExecutionContext
{
    public string PipelineId { get; }
    public string CorrelationId { get; }
    public int Step { get; private set; }
    public ConcurrentDictionary<string, object?> Data { get; }
    public List<DecisionTrace> DecisionTrace { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    public ExecutionContext(string pipelineId)
    {
        PipelineId = pipelineId;
        CorrelationId = Guid.NewGuid().ToString();
        Step = 0;
        Data = new ConcurrentDictionary<string, object?>();
        DecisionTrace = new List<DecisionTrace>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    [JsonConstructor]
    public ExecutionContext(
        string pipelineId,
        string correlationId,
        int step,
        ConcurrentDictionary<string, object?> data,
        List<DecisionTrace> decisionTrace,
        DateTime createdAt,
        DateTime updatedAt)
    {
        PipelineId = pipelineId;
        CorrelationId = correlationId;
        Step = step;
        Data = data ?? new ConcurrentDictionary<string, object?>();
        DecisionTrace = decisionTrace ?? new List<DecisionTrace>();
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public void RecordDecision(DecisionTrace trace)
    {
        Interlocked.Increment(ref Step);
        lock (DecisionTrace)
        {
            DecisionTrace.Add(trace);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyList<DecisionTrace> GetTrace()
    {
        lock (DecisionTrace)
        {
            return DecisionTrace.ToArray();
        }
    }

    /// <summary>
    /// Thread-safe get or add for context data.
    /// </summary>
    public T? GetData<T>(string key) where T : class
    {
        if (Data.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return null;
    }

    public T GetDataOrDefault<T>(string key, T defaultValue) where T : notnull
    {
        if (Data.TryGetValue(key, out var value) && value is T typed)
            return typed;
        return defaultValue;
    }
}

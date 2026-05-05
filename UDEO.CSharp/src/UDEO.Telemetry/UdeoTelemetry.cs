using System.Collections.Concurrent;
using System.Text.Json;
using UDEO.Core.Logging;

namespace UDEO.Telemetry;

/// <summary>
/// Observability layer: distributed tracing spans, counters, histograms, and audit logging.
/// Replaces the PowerShell UDEOTelemetry class.
/// </summary>
public sealed class UdeoTelemetry
{
    private static readonly Lazy<UdeoTelemetry> _instance = new(() => new UdeoTelemetry());
    public static UdeoTelemetry Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, ConcurrentBag<SpanRecord>> _spans = new();
    private readonly ConcurrentDictionary<string, double> _counters = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<double>> _histograms = new();
    private string? _auditPath;
    private readonly object _auditLock = new();

    private UdeoTelemetry() { }

    #region Tracing

    /// <summary>
    /// Starts a named span and returns a span ID for later completion.
    /// </summary>
    public string StartSpan(string name)
    {
        var id = Guid.NewGuid().ToString()[..8];
        var span = new SpanRecord
        {
            Id = id,
            Name = name,
            StartedAt = DateTime.UtcNow
        };

        _spans.GetOrAdd(name, _ => new ConcurrentBag<SpanRecord>()).Add(span);
        return id;
    }

    /// <summary>
    /// Ends a span by name and ID.
    /// </summary>
    public void EndSpan(string name, string id)
    {
        if (_spans.TryGetValue(name, out var bag))
        {
            foreach (var span in bag)
            {
                if (span.Id == id && span.EndedAt == null)
                {
                    span.EndedAt = DateTime.UtcNow;
                    span.DurationMs = Math.Round((span.EndedAt.Value - span.StartedAt).TotalMilliseconds, 2);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Wraps an action in a span, auto-completing on exit.
    /// </summary>
    public void Trace(string name, Action action)
    {
        var id = StartSpan(name);
        try { action(); }
        finally { EndSpan(name, id); }
    }

    /// <summary>
    /// Wraps an async action in a span.
    /// </summary>
    public async Task TraceAsync(string name, Func<Task> action)
    {
        var id = StartSpan(name);
        try { await action(); }
        finally { EndSpan(name, id); }
    }

    #endregion

    #region Metrics

    /// <summary>
    /// Increments a named counter by a delta.
    /// </summary>
    public void Inc(string name, double delta = 1)
        => _counters.AddOrUpdate(name, delta, (_, existing) => existing + delta);

    /// <summary>
    /// Records a value for a named histogram.
    /// </summary>
    public void Record(string name, double value)
        => _histograms.GetOrAdd(name, _ => new ConcurrentBag<double>()).Add(value);

    #endregion

    #region Audit

    /// <summary>
    /// Initializes the audit log path.
    /// </summary>
    public void InitializeAudit(string path)
    {
        _auditPath = path;
        var dir = Path.GetDirectoryName(path);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    /// <summary>
    /// Writes an audit event with details.
    /// </summary>
    public void Audit(string @event, object? details = null)
    {
        if (_auditPath == null) return;

        var entry = new
        {
            Timestamp = DateTime.UtcNow.ToString("O"),
            Event = @event,
            Details = details
        };

        lock (_auditLock)
        {
            try
            {
                var json = JsonSerializer.Serialize(entry);
                File.AppendAllText(_auditPath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                UdeoLogger.Instance.Debug($"Audit write failed: {ex.Message}");
            }
        }
    }

    #endregion

    #region Summary

    /// <summary>
    /// Returns a comprehensive telemetry summary.
    /// </summary>
    public TelemetrySummary GetSummary()
    {
        var spanSummaries = _spans.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var completed = kvp.Value.Where(s => s.DurationMs.HasValue).ToList();
                return new SpanSummary
                {
                    Count = kvp.Value.Count,
                    TotalMs = Math.Round(completed.Sum(s => s.DurationMs ?? 0), 2),
                    AvgMs = Math.Round(completed.Count > 0 ? completed.Average(s => s.DurationMs ?? 0) : 0, 2)
                };
            });

        var histogramSummaries = _histograms.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var vals = kvp.Value.ToList();
                return new HistogramSummary
                {
                    Count = vals.Count,
                    Avg = Math.Round(vals.Count > 0 ? vals.Average() : 0, 2),
                    Min = vals.Count > 0 ? vals.Min() : 0,
                    Max = vals.Count > 0 ? vals.Max() : 0
                };
            });

        return new TelemetrySummary
        {
            Spans = spanSummaries,
            Counters = _counters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Histograms = histogramSummaries
        };
    }

    #endregion

    #region Reset

    /// <summary>
    /// Resets all telemetry data.
    /// </summary>
    public void Reset()
    {
        _spans.Clear();
        _counters.Clear();
        _histograms.Clear();
    }

    #endregion
}

public sealed class SpanRecord
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; set; }
    public double? DurationMs { get; set; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
}

public sealed class SpanSummary
{
    public int Count { get; set; }
    public double TotalMs { get; set; }
    public double AvgMs { get; set; }
}

public sealed class HistogramSummary
{
    public int Count { get; set; }
    public double Avg { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}

public sealed class TelemetrySummary
{
    public Dictionary<string, SpanSummary> Spans { get; set; } = new();
    public Dictionary<string, double> Counters { get; set; } = new();
    public Dictionary<string, HistogramSummary> Histograms { get; set; } = new();
}

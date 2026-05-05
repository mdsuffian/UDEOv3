using System.Text.Json;
using UDEO.Core.Configuration;
using UDEO.Core.Logging;
using UDEO.Core.Models;

namespace UDEO.Store;

/// <summary>
/// JSON file-based state persistence for pipeline runs.
/// Replaces the PowerShell UDEOStore class.
/// </summary>
public sealed class UdeoStore
{
    private static readonly Lazy<UdeoStore> _instance = new(() => new UdeoStore());
    public static UdeoStore Instance => _instance.Value;

    private static readonly JsonSerializerOptions StoreOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private string? _rootPath;
    private bool _isReady;
    private readonly object _lock = new();

    private UdeoStore() { }

    /// <summary>
    /// Initializes the store at the given directory.
    /// </summary>
    public void Initialize(string? rootPath = null)
    {
        _rootPath = rootPath ?? UdeoConfig.Instance.GetString("storePath");
        if (!Directory.Exists(_rootPath))
            Directory.CreateDirectory(_rootPath!);
        _isReady = true;
        UdeoLogger.Instance.Debug($"Store initialized: {_rootPath}");
    }

    /// <summary>
    /// Saves a pipeline execution context to JSON.
    /// </summary>
    public void Save(ExecutionContext context)
    {
        if (!GuardReady(context.PipelineId)) return;

        var file = Path.Combine(_rootPath!, $"{context.PipelineId}.json");
        try
        {
            var json = JsonSerializer.Serialize(context, StoreOptions);
            lock (_lock)
            {
                File.WriteAllText(file, json);
            }
            UdeoLogger.Instance.Debug($"Saved: {file}");
        }
        catch (Exception ex)
        {
            UdeoLogger.Instance.Error($"Failed to save context {context.PipelineId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a saved pipeline context from JSON.
    /// </summary>
    public ExecutionContext? Load(string pipelineId)
    {
        if (!GuardReady(pipelineId)) return null;

        var file = Path.Combine(_rootPath!, $"{pipelineId}.json");
        if (!File.Exists(file))
        {
            UdeoLogger.Instance.Debug($"Context not found: {pipelineId}");
            return null;
        }

        try
        {
            var json = File.ReadAllText(file);
            return JsonSerializer.Deserialize<ExecutionContext>(json, StoreOptions);
        }
        catch (Exception ex)
        {
            UdeoLogger.Instance.Error($"Failed to load context {pipelineId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Lists all saved pipeline IDs.
    /// </summary>
    public string[] List()
    {
        if (!_isReady || _rootPath == null || !Directory.Exists(_rootPath))
            return Array.Empty<string>();

        return Directory.GetFiles(_rootPath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(f => f != null)
            .Cast<string>()
            .ToArray();
    }

    /// <summary>
    /// Deletes a single pipeline run.
    /// </summary>
    public bool Delete(string pipelineId)
    {
        if (!_isReady || _rootPath == null) return false;

        var file = Path.Combine(_rootPath, $"{pipelineId}.json");
        if (!File.Exists(file)) return false;

        lock (_lock)
        {
            File.Delete(file);
        }
        UdeoLogger.Instance.Debug($"Deleted: {pipelineId}");
        return true;
    }

    /// <summary>
    /// Purges all saved pipeline runs.
    /// </summary>
    public int Purge()
    {
        if (!_isReady || _rootPath == null || !Directory.Exists(_rootPath))
            return 0;

        var files = Directory.GetFiles(_rootPath, "*.json");
        lock (_lock)
        {
            foreach (var file in files)
                File.Delete(file);
        }

        UdeoLogger.Instance.Info($"Store purged: {files.Length} files removed");
        return files.Length;
    }

    private bool GuardReady(string? id = null)
    {
        if (_isReady) return true;

        UdeoLogger.Instance.Warn($"Store not initialized, cannot save context {id ?? "unknown"}");
        return false;
    }
}

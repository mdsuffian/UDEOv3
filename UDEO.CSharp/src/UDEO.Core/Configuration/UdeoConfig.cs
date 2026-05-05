using System.Text.Json;

namespace UDEO.Core.Configuration;

/// <summary>
/// Loads and manages UDEO configuration from .udeo/config.json with defaults.
/// Replaces the PowerShell UDEOConfig class.
/// </summary>
public sealed class UdeoConfig
{
    private static readonly Lazy<UdeoConfig> _instance = new(() => new UdeoConfig());
    public static UdeoConfig Instance => _instance.Value;

    private readonly Dictionary<string, object?> _data;
    private readonly string? _workspaceRoot;

    public string WorkspaceRoot => _workspaceRoot ?? Directory.GetCurrentDirectory();

    private UdeoConfig()
    {
        _workspaceRoot = Environment.GetEnvironmentVariable("UDEO_WORKSPACE")
                         ?? Directory.GetCurrentDirectory();

        _data = BuildDefaultConfig();

        // Load workspace overrides
        var configFile = Path.Combine(_workspaceRoot, ".udeo", "config.json");
        if (File.Exists(configFile))
        {
            try
            {
                var json = File.ReadAllText(configFile);
                var overrideConfig = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (overrideConfig != null)
                {
                    MergeOverride(overrideConfig, _data);
                }
            }
            catch (Exception ex)
            {
                UdeoLogger.Instance.Debug($"Failed to parse .udeo/config.json, using defaults: {ex.Message}");
            }
        }

        UdeoLogger.Instance.Debug($"UDEO config loaded: workspace={WorkspaceRoot}");
    }

    public void Reload()
    {
        var defaults = BuildDefaultConfig();
        foreach (var kvp in defaults)
            _data[kvp.Key] = kvp.Value;

        var configFile = Path.Combine(_workspaceRoot ?? ".", ".udeo", "config.json");
        if (File.Exists(configFile))
        {
            try
            {
                var json = File.ReadAllText(configFile);
                var overrideConfig = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (overrideConfig != null)
                    MergeOverride(overrideConfig, _data);
            }
            catch { }
        }
    }

    public object? Get(string path)
    {
        var parts = path.Split('.');
        object? current = _data;

        foreach (var part in parts)
        {
            if (current is Dictionary<string, object?> dict && dict.TryGetValue(part, out var next))
                current = next;
            else
                return null;
        }

        return current;
    }

    public T? Get<T>(string path) where T : class
        => Get(path) as T;

    public string GetString(string path, string defaultValue = "")
        => Get(path)?.ToString() ?? defaultValue;

    public int GetInt(string path, int defaultValue = 0)
        => Get(path) is int i ? i :
           int.TryParse(Get(path)?.ToString(), out var parsed) ? parsed : defaultValue;

    public bool GetBool(string path, bool defaultValue = false)
        => Get(path) is bool b ? b :
           bool.TryParse(Get(path)?.ToString(), out var parsed) ? parsed : defaultValue;

    private Dictionary<string, object?> BuildDefaultConfig()
    {
        var workspace = _workspaceRoot ?? Directory.GetCurrentDirectory();
        return new Dictionary<string, object?>
        {
            ["version"] = "3.1.0",
            ["storePath"] = Path.Combine(workspace, ".udeo", "store"),
            ["logLevel"] = "Info",
            ["quiet"] = false,
            ["experts"] = new Dictionary<string, object?>
            {
                ["timeoutSeconds"] = 30,
                ["pluginDirectory"] = Path.Combine(workspace, "plugins")
            },
            ["pipeline"] = new Dictionary<string, object?>
            {
                ["maxRetries"] = 2,
                ["defaultTimeout"] = 60,
                ["autoEscalate"] = true
            }
        };
    }

    private static void MergeOverride(Dictionary<string, JsonElement> source, Dictionary<string, object?> target)
    {
        foreach (var kvp in source)
        {
            if (target.ContainsKey(kvp.Key) && target[kvp.Key] is Dictionary<string, object?> nestedTarget
                && kvp.Value.ValueKind == JsonValueKind.Object)
            {
                var nestedSource = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(kvp.Value.GetRawText());
                if (nestedSource != null)
                    MergeOverride(nestedSource, nestedTarget);
            }
            else
            {
                target[kvp.Key] = ConvertJsonElement(kvp.Value);
            }
        }
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => element.TryGetInt32(out var i) ? (object)i : element.GetDouble(),
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText()),
            _ => null
        };
    }
}

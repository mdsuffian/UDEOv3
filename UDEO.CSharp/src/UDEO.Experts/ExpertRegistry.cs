using System.Collections.Concurrent;
using UDEO.Core;
using UDEO.Core.Logging;
using UDEO.Core.Models;

namespace UDEO.Experts;

/// <summary>
/// Thread-safe registry of all registered experts.
/// Replaces the PowerShell UDEOExpertRegistry class.
/// </summary>
public sealed class ExpertRegistry
{
    private static readonly Lazy<ExpertRegistry> _instance = new(() => new ExpertRegistry());
    public static ExpertRegistry Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, ExpertContract> _experts = new(StringComparer.OrdinalIgnoreCase);

    private ExpertRegistry() { }

    public void Register(ExpertContract contract)
    {
        _experts[contract.Id] = contract;
        UdeoLogger.Instance.Info($"Registered expert: {contract.Id} [{contract.Type}]",
            new { contract.Name, contract.Version });
    }

    public bool Unregister(string id)
    {
        if (_experts.TryRemove(id, out _))
        {
            UdeoLogger.Instance.Info($"Unregistered expert: {id}");
            return true;
        }
        return false;
    }

    public ExpertContract? Get(string id)
    {
        _experts.TryGetValue(id, out var contract);
        return contract;
    }

    public IReadOnlyList<ExpertContract> GetAll()
        => _experts.Values.ToList().AsReadOnly();

    public IReadOnlyList<ExpertContract> GetByType(UdeoExpertType type)
        => _experts.Values.Where(e => e.Type == type).ToList().AsReadOnly();

    public int Count => _experts.Count;

    /// <summary>
    /// Discovers and loads expert plugins from a directory.
    /// Uses the plugin loader for .dll files.
    /// </summary>
    public void DiscoverPlugins(string pluginDir)
    {
        if (!Directory.Exists(pluginDir))
        {
            UdeoLogger.Instance.Debug($"Plugin directory not found: {pluginDir}");
            return;
        }

        UdeoLogger.Instance.Info($"Discovering plugins in: {pluginDir}");

        // Load .dll plugins (compiled assemblies)
        foreach (var dll in Directory.GetFiles(pluginDir, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                PluginLoader.LoadAssembly(dll);
                UdeoLogger.Instance.Debug($"Loaded plugin assembly: {Path.GetFileName(dll)}");
            }
            catch (Exception ex)
            {
                UdeoLogger.Instance.Warn($"Failed to load plugin {Path.GetFileName(dll)}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clears all registered experts.
    /// </summary>
    public void Clear()
    {
        _experts.Clear();
    }
}

using System.Reflection;
using UDEO.Core.Logging;

namespace UDEO.Experts;

/// <summary>
/// Loads expert plugin assemblies at runtime.
/// </summary>
public static class PluginLoader
{
    private static readonly HashSet<string> _loadedAssemblies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Loads an assembly from a .dll file and calls any static initialization.
    /// </summary>
    public static Assembly LoadAssembly(string dllPath)
    {
        var fullPath = Path.GetFullPath(dllPath);
        if (_loadedAssemblies.Contains(fullPath))
        {
            UdeoLogger.Instance.Debug($"Assembly already loaded: {fullPath}");
            return Assembly.LoadFrom(fullPath);
        }

        var assembly = Assembly.LoadFrom(fullPath);
        _loadedAssemblies.Add(fullPath);

        // Look for types implementing IExternalExpert
        foreach (var type in assembly.GetExportedTypes())
        {
            if (typeof(IExternalExpert).IsAssignableFrom(type) && !type.IsAbstract)
            {
                try
                {
                    var expert = (IExternalExpert)Activator.CreateInstance(type)!;
                    expert.Register();
                    UdeoLogger.Instance.Info($"Registered external expert plugin: {type.Name}");
                }
                catch (Exception ex)
                {
                    UdeoLogger.Instance.Warn($"Failed to instantiate expert plugin {type.Name}: {ex.Message}");
                }
            }
        }

        return assembly;
    }

    /// <summary>
    /// Scans a directory for assemblies implementing IExternalExpert.
    /// </summary>
    public static int DiscoverAndLoad(string directory)
    {
        if (!Directory.Exists(directory)) return 0;

        var count = 0;
        foreach (var dll in Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                LoadAssembly(dll);
                count++;
            }
            catch (Exception ex)
            {
                UdeoLogger.Instance.Warn($"Failed to load assembly {Path.GetFileName(dll)}: {ex.Message}");
            }
        }

        return count;
    }
}

/// <summary>
/// Interface for external expert plugins to auto-register.
/// </summary>
public interface IExternalExpert
{
    void Register();
}

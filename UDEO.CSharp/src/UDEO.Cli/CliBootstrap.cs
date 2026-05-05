using UDEO.Core.Configuration;
using UDEO.Core.Logging;
using UDEO.Experts;
using UDEO.Experts.BuiltIn;
using UDEO.Store;

namespace UDEO.Cli;

/// <summary>
/// Bootstraps the UDEO framework on CLI startup.
/// </summary>
public static class CliBootstrap
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        var workspace = Environment.GetEnvironmentVariable("UDEO_WORKSPACE")
                        ?? Directory.GetCurrentDirectory();

        // Load configuration
        UdeoConfig.Instance.Reload();

        // Configure logger
        var logLevel = UdeoConfig.Instance.GetString("logLevel", "Info");
        var logFile = Path.Combine(workspace, ".udeo", "udeo.log");
        var quiet = UdeoConfig.Instance.GetBool("quiet", false);
        UdeoLogger.Instance.Configure(logLevel, logFile, quiet);

        // Initialize store
        var storePath = UdeoConfig.Instance.GetString("storePath",
            Path.Combine(workspace, ".udeo", "store"));
        UdeoStore.Instance.Initialize(storePath);

        // Register built-in experts
        ValidationExpert.Register();
        MathExpert.Register();
        RiskExpert.Register();
        HumanReviewExpert.Register();

        // Discover plugins
        var pluginDir = UdeoConfig.Instance.GetString("experts.pluginDirectory",
            Path.Combine(workspace, "plugins"));
        ExpertRegistry.Instance.DiscoverPlugins(pluginDir);

        UdeoLogger.Instance.Debug($"UDEO CLI bootstrapped: workspace={workspace}, experts={ExpertRegistry.Instance.Count}");
    }
}

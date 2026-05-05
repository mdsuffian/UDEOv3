using System.CommandLine;
using System.Text.Json;
using UDEO.Core.Logging;

namespace UDEO.Cli.Commands;

/// <summary>
/// udeo init — initializes a UDEO workspace.
/// </summary>
public static class InitCommand
{
    public static Command Create()
    {
        var cmd = new Command("init", "Initialize a UDEO workspace");
        cmd.SetHandler(Execute);
        return cmd;
    }

    private static void Execute()
    {
        CliBootstrap.Initialize();
        var workspace = Environment.GetEnvironmentVariable("UDEO_WORKSPACE")
                        ?? Directory.GetCurrentDirectory();

        WriteColored("Initializing UDEO workspace at: " + workspace, ConsoleColor.Cyan);

        var udeoDir = Path.Combine(workspace, ".udeo");
        var storeDir = Path.Combine(udeoDir, "store");
        Directory.CreateDirectory(storeDir);

        var configFile = Path.Combine(udeoDir, "config.json");
        if (!File.Exists(configFile))
        {
            var defaultConfig = new Dictionary<string, object?>
            {
                ["logLevel"] = "Info",
                ["quiet"] = false,
                ["pipeline"] = new Dictionary<string, object?>
                {
                    ["autoEscalate"] = true,
                    ["defaultTimeout"] = 60
                }
            };
            var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configFile, json);
            WriteColored($"  Created: {configFile}", ConsoleColor.Green);
        }
        else
        {
            WriteColored($"  Config already exists: {configFile}", ConsoleColor.Yellow);
        }

        var pluginsDir = Path.Combine(workspace, "plugins");
        if (!Directory.Exists(pluginsDir))
        {
            Directory.CreateDirectory(pluginsDir);
            WriteColored($"  Created: {pluginsDir}", ConsoleColor.Green);
        }

        WriteColored("UDEO workspace ready.", ConsoleColor.Green);
        WriteColored("Next: udeo run loan-approval", ConsoleColor.Cyan);
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}

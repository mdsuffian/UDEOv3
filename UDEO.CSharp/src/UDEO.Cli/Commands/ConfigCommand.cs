using System.CommandLine;
using UDEO.Core.Configuration;

namespace UDEO.Cli.Commands;

/// <summary>
/// udeo config — show current configuration.
/// </summary>
public static class ConfigCommand
{
    public static Command Create()
    {
        var cmd = new Command("config", "Show UDEO configuration");
        cmd.SetHandler(Execute);
        return cmd;
    }

    private static void Execute()
    {
        CliBootstrap.Initialize();
        var config = UdeoConfig.Instance;

        WriteColored("\nUDEO Configuration:", ConsoleColor.Cyan);
        Console.WriteLine($"  Workspace: {config.WorkspaceRoot}");
        Console.WriteLine($"  Store:     {config.GetString("storePath")}");
        Console.WriteLine($"  LogLevel:  {config.GetString("logLevel")}");
        Console.WriteLine($"  Plugins:   {config.GetString("experts.pluginDirectory")}");
        Console.WriteLine($"\nFull config: {Path.Combine(config.WorkspaceRoot, ".udeo", "config.json")}");
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}

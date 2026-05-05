using System.CommandLine;

namespace UDEO.Cli.Commands;

/// <summary>
/// udeo version — show version info.
/// </summary>
public static class VersionCommand
{
    public static Command Create()
    {
        var cmd = new Command("version", "Show UDEO version");
        cmd.SetHandler(() =>
        {
            WriteColored("UDEO v3.1.0 — Universal Deterministic Expert Orchestrator", ConsoleColor.Cyan);
            WriteColored("Zero-dependency, pluggable expert pipeline framework.", ConsoleColor.White);
            WriteColored("Built with .NET 8.0 and C++ native extensions.", ConsoleColor.DarkGray);
        });
        return cmd;
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}

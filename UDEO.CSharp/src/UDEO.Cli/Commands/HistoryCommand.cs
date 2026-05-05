using System.CommandLine;
using UDEO.Store;

namespace UDEO.Cli.Commands;

/// <summary>
/// udeo history — view past pipeline runs.
/// </summary>
public static class HistoryCommand
{
    public static Command Create()
    {
        var cmd = new Command("history", "View past pipeline runs");
        cmd.SetHandler(Execute);
        return cmd;
    }

    private static void Execute()
    {
        CliBootstrap.Initialize();
        var runs = UdeoStore.Instance.List();

        WriteColored("\nPast Runs:", ConsoleColor.Cyan);

        if (runs.Length == 0)
        {
            WriteColored("  No runs found.", ConsoleColor.DarkGray);
            return;
        }

        foreach (var id in runs)
        {
            var data = UdeoStore.Instance.Load(id);
            if (data == null) continue;

            var decision = data.DecisionTrace.Count > 0
                ? data.DecisionTrace[^1].DecisionCode
                : "UNKNOWN";
            var ts = data.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"  {id}  [{decision}]  {ts}");
        }

        WriteColored("\nView a specific run: udeo history show <id>", ConsoleColor.DarkGray);
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using UDEO.Experts;

namespace UDEO.Cli.Commands;

/// <summary>
/// udeo expert — manage registered experts.
/// </summary>
public static class ExpertCommand
{
    public static Command Create()
    {
        var cmd = new Command("expert", "Manage experts");

        cmd.AddCommand(CreateListCommand());
        cmd.AddCommand(CreateRegisterCommand());

        return cmd;
    }

    private static Command CreateListCommand()
    {
        var cmd = new Command("list", "List all registered experts");
        cmd.SetHandler(() =>
        {
            CliBootstrap.Initialize();
            var experts = ExpertRegistry.Instance.GetAll();

            WriteColored("\nRegistered Experts:", ConsoleColor.Cyan);
            Console.WriteLine($"{"Id",-24} {"Name",-28} {"Type",-16} {"Version",-10}");
            Console.WriteLine(new string('-', 78));
            foreach (var e in experts)
            {
                Console.WriteLine($"{e.Id,-24} {e.Name,-28} {e.Type,-16} {e.Version,-10}");
            }
            Console.WriteLine($"\nTotal: {experts.Count} experts");
        });
        return cmd;
    }

    private static Command CreateRegisterCommand()
    {
        var cmd = new Command("register", "Register a custom expert");
        cmd.SetHandler(() =>
        {
            CliBootstrap.Initialize();
            WriteColored("To register a custom expert, create a plugin DLL implementing IExternalExpert.", ConsoleColor.Yellow);
            WriteColored("Place the DLL in: plugins/", ConsoleColor.Yellow);
            WriteColored("See documentation for the IExternalExpert interface.", ConsoleColor.Yellow);
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

using System.CommandLine;
using System.CommandLine.Invocation;
using UDEO.Cli.Commands;

namespace UDEO.Cli;

/// <summary>
/// UDEO CLI entry point — Universal Deterministic Expert Orchestrator v3.1.0
/// Rewritten from PowerShell to C# with System.CommandLine.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Early bootstrap: load config, initialize store, register built-in experts
        CliBootstrap.Initialize();

        var rootCommand = new RootCommand("UDEO v3.1.0 — Universal Deterministic Expert Orchestrator");

        rootCommand.AddCommand(InitCommand.Create());
        rootCommand.AddCommand(RunCommand.Create());
        rootCommand.AddCommand(ExpertCommand.Create());
        rootCommand.AddCommand(HistoryCommand.Create());
        rootCommand.AddCommand(ConfigCommand.Create());
        rootCommand.AddCommand(VersionCommand.Create());

        return await rootCommand.InvokeAsync(args);
    }
}

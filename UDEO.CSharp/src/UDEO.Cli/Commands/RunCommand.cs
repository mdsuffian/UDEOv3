using System.CommandLine;
using UDEO.Core.Logging;
using UDEO.Experts;
using UDEO.Pipeline;
using UDEO.Pipeline.Templates;
using UDEO.Store;

namespace UDEO.Cli.Commands;

/// <summary>
/// udeo run — executes a named pipeline.
/// </summary>
public static class RunCommand
{
    public static Command Create()
    {
        var cmd = new Command("run", "Run a pipeline");

        var pipelineArg = new Argument<string>("pipeline", "Pipeline name (loan-approval, custom, etc.)");
        cmd.AddArgument(pipelineArg);

        var incomeOpt = new Option<double>("--Income", () => 75000, "Annual income");
        var creditScoreOpt = new Option<int>("--CreditScore", () => 720, "Credit score");
        var loanAmountOpt = new Option<double>("--LoanAmount", () => 300000, "Loan amount");
        var debtOpt = new Option<double>("--Debt", () => 25000, "Annual debt");
        var rateOpt = new Option<double>("--InterestRate", () => 0.065, "Interest rate");
        var termOpt = new Option<int>("--TermMonths", () => 360, "Loan term in months");
        var propertyOpt = new Option<double>("--PropertyValue", () => 375000, "Property value");

        cmd.AddOption(incomeOpt);
        cmd.AddOption(creditScoreOpt);
        cmd.AddOption(loanAmountOpt);
        cmd.AddOption(debtOpt);
        cmd.AddOption(rateOpt);
        cmd.AddOption(termOpt);
        cmd.AddOption(propertyOpt);

        cmd.SetHandler(Execute, pipelineArg, incomeOpt, creditScoreOpt, loanAmountOpt,
            debtOpt, rateOpt, termOpt, propertyOpt);

        return cmd;
    }

    private static void Execute(
        string pipelineName,
        double income, int creditScore, double loanAmount,
        double debt, double rate, int term, double propertyValue)
    {
        CliBootstrap.Initialize();

        UdeoPipeline? pipeline = null;

        switch (pipelineName.ToLowerInvariant())
        {
            case "loan-approval":
            case "loan":
                pipeline = LoanApprovalPipeline.Create(
                    income, debt, creditScore, loanAmount, rate, term, propertyValue);
                break;
            default:
                WriteColored($"Unknown pipeline: {pipelineName}", ConsoleColor.Red);
                WriteColored("Available: loan-approval", ConsoleColor.Yellow);
                return;
        }

        WriteColored($"\n=== UDEO Pipeline: {pipeline!.Name} ===", ConsoleColor.Cyan);
        WriteColored($"Pipeline ID: {pipeline.Id}", ConsoleColor.DarkGray);

        var result = pipeline.Run();

        WriteColored("\n=== RESULT ===", ConsoleColor.Cyan);
        PrintDecision(result.Decision);
        if (result.Reason != null)
            Console.WriteLine($"  Reason: {result.Reason}");
        Console.WriteLine($"  Steps: {result.Trace.Count}");

        // Print trace
        WriteColored("\n=== DECISION TRACE ===", ConsoleColor.Cyan);
        Console.WriteLine($"{"ExpertId",-22} {"DecisionCode",-14} {"RuleFired",-50} {"Time(ms)",10}");
        Console.WriteLine(new string('-', 96));
        foreach (var t in result.Trace)
        {
            Console.WriteLine($"{t.ExpertId,-22} {t.DecisionCode,-14} {t.RuleFired,-50} {t.ExecutionTimeMs,10:F2}");
        }

        // Save to store
        if (result.Context != null)
        {
            UdeoStore.Instance.Save(result.Context);
            WriteColored($"Saved run: {result.Context.PipelineId}", ConsoleColor.DarkGray);
        }
    }

    private static void PrintDecision(string decision)
    {
        var (text, color) = decision switch
        {
            "APPROVED" => ("  Decision: APPROVED", ConsoleColor.Green),
            "REJECTED" => ("  Decision: REJECTED", ConsoleColor.Red),
            "FLAGGED" => ("  Decision: FLAGGED", ConsoleColor.Yellow),
            "ROUTE_TO_HUMAN" => ("  Decision: ROUTE TO HUMAN", ConsoleColor.Magenta),
            _ => ($"  Decision: {decision}", ConsoleColor.White)
        };

        WriteColored(text, color);
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}

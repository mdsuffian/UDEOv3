using UDEO.Core;
using UDEO.Core.Configuration;
using UDEO.Core.Logging;
using UDEO.Core.Models;
using UDEO.Experts;
using UDEO.Pipeline;
using UDEO.Telemetry;

// Bootstrap
UdeoConfig.Instance.Reload();
UdeoLogger.Instance.Configure("Info", null, false);

Console.WriteLine("=== UDEO Quickstart Example ===");
Console.WriteLine();

// Register a custom inline expert
ExpertRegistry.Instance.Register(new ExpertContract("greeter", "Greeting Expert",
    UdeoExpertType.Custom,
    (ctx, parameters) =>
    {
        var name = parameters.GetValueOrDefault("Name")?.ToString() ?? "World";
        UdeoLogger.Instance.Info($"  Greeter says: Hello, {name}!");
        ctx.Data["greeting"] = $"Hello, {name}!";
        return ExpertResult.SuccessResult("VALID", "GREETING_DELIVERED");
    }));

// Create and run a pipeline
var pipeline = new UdeoPipeline("Quickstart");
pipeline.Context.Data["message"] = "UDEO works!";
pipeline.AddStep("greeter", new Dictionary<string, object?> { ["Name"] = "World" });
pipeline.AddStep("udeo.validation", new Dictionary<string, object?>
{
    ["Field"] = "message",
    ["Schema"] = "non_empty_string",
    ["Required"] = true
});

// Register validation expert since QuickStart doesn't use CliBootstrap
UDEO.Experts.BuiltIn.ValidationExpert.Register();

var result = pipeline.Run();

Console.WriteLine();
Console.WriteLine($"=== Result ===");
Console.WriteLine($"Decision: {result.Decision}");
Console.WriteLine($"Trace entries: {result.Trace.Count}");
if (result.Context?.Data.TryGetValue("greeting", out var greeting))
    Console.WriteLine($"Context greeting: {greeting}");

// Show telemetry
var telemetry = UdeoTelemetry.Instance.GetSummary();
Console.WriteLine();
Console.WriteLine("=== Telemetry ===");
Console.WriteLine($"Spans: {telemetry.Spans.Count}");
Console.WriteLine($"Counters: {telemetry.Counters.Count}");

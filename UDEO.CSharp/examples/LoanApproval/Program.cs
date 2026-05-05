using UDEO.Core.Configuration;
using UDEO.Core.Logging;
using UDEO.Experts.BuiltIn;
using UDEO.Pipeline.Templates;
using UDEO.Store;

// Bootstrap
UdeoConfig.Instance.Reload();
UdeoLogger.Instance.Configure("Error", null, false);

// Register experts
ValidationExpert.Register();
MathExpert.Register();
RiskExpert.Register();
HumanReviewExpert.Register();

Console.WriteLine("=== UDEO Loan Approval Pipeline ===");
Console.WriteLine();

// Create pipeline
var pipeline = LoanApprovalPipeline.Create(
    income: 120000,
    debt: 30000,
    creditScore: 750,
    loanAmount: 450000,
    interestRate: 0.0625,
    termMonths: 360,
    propertyValue: 550000);

// Run
var result = pipeline.Run();

// Display
Console.WriteLine();
Console.WriteLine("=== Pipeline Result ===");
Console.WriteLine($"Decision: {result.Decision}");
if (result.Reason != null)
    Console.WriteLine($"Reason: {result.Reason}");

Console.WriteLine();
Console.WriteLine("=== Calculations ===");
if (result.Context?.Data.TryGetValue("calculations", out var calcs) && calcs is Dictionary<string, object> calcDict)
{
    foreach (var kvp in calcDict)
        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
}

Console.WriteLine();
Console.WriteLine("=== Decision Trace ===");
Console.WriteLine($"{"ExpertId",-22} {"DecisionCode",-14} {"RuleFired",-50} {"Time(ms)",10}");
foreach (var t in result.Trace)
    Console.WriteLine($"{t.ExpertId,-22} {t.DecisionCode,-14} {t.RuleFired,-50} {t.ExecutionTimeMs,10:F2}");

// Save
UdeoStore.Instance.Initialize();
if (result.Context != null)
    UdeoStore.Instance.Save(result.Context);
Console.WriteLine($"\nRun saved as: {result.Context?.PipelineId}");

// Test scenarios
Console.WriteLine("\n=== Additional Scenarios ===");

static void TestScenario(string label, double income, double debt, int creditScore,
    double loanAmount, double propertyValue)
{
    Console.WriteLine($"\n--- {label} ---");
    var p = LoanApprovalPipeline.Create(income, debt, creditScore, loanAmount, 0.065, 360, propertyValue);
    var r = p.Run();
    Console.WriteLine($"Decision: {r.Decision} — {r.Reason}");
}

TestScenario("High DTI (should flag)", 50000, 35000, 700, 200000, 250000);
TestScenario("Bad Credit (should reject)", 80000, 10000, 550, 150000, 200000);
TestScenario("Perfect Applicant (should approve)", 200000, 5000, 820, 300000, 500000);

Console.WriteLine("\n=== Done ===");

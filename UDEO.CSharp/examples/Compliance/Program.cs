using UDEO.Core;
using UDEO.Core.Configuration;
using UDEO.Core.Logging;
using UDEO.Core.Models;
using UDEO.Experts;
using UDEO.Experts.BuiltIn;
using UDEO.Pipeline;

// Bootstrap
UdeoConfig.Instance.Reload();
UdeoLogger.Instance.Configure("Error", null, false);

ValidationExpert.Register();
HumanReviewExpert.Register();

Console.WriteLine("=== UDEO Compliance Check Example ===");
Console.WriteLine();

// Register KYC expert
ExpertRegistry.Instance.Register(new ExpertContract("kyc_check", "KYC Verification Expert",
    UdeoExpertType.Validation,
    (ctx, parameters) =>
    {
        if (!ctx.Data.TryGetValue("applicant", out var appObj) || appObj is not Dictionary<string, object> applicant)
            return ExpertResult.FailureResult("Missing applicant data");

        var issues = new List<string>();
        var name = applicant.GetValueOrDefault("name")?.ToString() ?? "";
        if (name.Length < 2) issues.Add("Invalid name");
        if (string.IsNullOrEmpty(applicant.GetValueOrDefault("id_number")?.ToString()))
            issues.Add("Missing ID number");

        var age = applicant.GetValueOrDefault("age");
        if (UDEO.Core.Extensions.UdeoExtensions.ToIntSafe(age) < 18)
            issues.Add("Under 18");

        var country = applicant.GetValueOrDefault("country")?.ToString() ?? "";
        if (country is "sanctioned_country_A" or "sanctioned_country_B")
            issues.Add("Sanctioned country");

        ctx.Data["kyc_result"] = new Dictionary<string, object>
        {
            ["passed"] = issues.Count == 0,
            ["issues"] = issues,
            ["checked_at"] = DateTime.UtcNow.ToString("O")
        };

        if (issues.Count > 0)
            return ExpertResult.SuccessResult("REJECTED", $"KYC_FAILED:{string.Join(", ", issues)}");
        return ExpertResult.SuccessResult("VALID", "KYC_PASSED");
    }));

// Register AML expert
ExpertRegistry.Instance.Register(new ExpertContract("aml_screening", "AML Screening Expert",
    UdeoExpertType.Risk,
    (ctx, parameters) =>
    {
        if (!ctx.Data.TryGetValue("applicant", out var appObj) || appObj is not Dictionary<string, object> applicant)
            return ExpertResult.FailureResult("Missing applicant data");

        var name = applicant.GetValueOrDefault("name")?.ToString() ?? "";
        var country = applicant.GetValueOrDefault("country")?.ToString() ?? "";
        var amount = UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(
            ctx.Data.TryGetValue("transaction_amount", out var ta) ? ta : 0) ?? 0;

        bool watchlistMatch = name.Contains("sanctioned", StringComparison.OrdinalIgnoreCase);
        bool highRiskCountry = country == "high_risk_country";
        bool largeTransaction = amount > 100000;

        if (watchlistMatch)
            return ExpertResult.SuccessResult("REJECTED", "WATCHLIST_MATCH");
        if (highRiskCountry && largeTransaction)
            return ExpertResult.SuccessResult("FLAGGED", "HIGH_RISK_COUNTRY_LARGE_TX");
        if (largeTransaction)
            return ExpertResult.SuccessResult("FLAGGED", "LARGE_TRANSACTION_REVIEW");
        return ExpertResult.SuccessResult("APPROVED", "AML_CLEAR");
    }));

// Build pipeline
var pipeline = new UdeoPipeline("Customer-Onboarding");
pipeline.Context.Data["applicant"] = new Dictionary<string, object?>
{
    ["name"] = "Alice Smithson",
    ["age"] = 34,
    ["id_number"] = "AB123456",
    ["country"] = "United Kingdom"
};
pipeline.Context.Data["transaction_amount"] = 50000;
pipeline.Context.Data["account_type"] = "personal_savings";

pipeline.AddStep("kyc_check");
pipeline.AddStep("aml_screening");
pipeline.AddStep("udeo.human", new Dictionary<string, object?>
{
    ["Reason"] = "Compliance flagged for review"
}, onFailure: FailurePolicy.Continue);

// Run
var result = pipeline.Run();

Console.WriteLine();
Console.WriteLine("=== Onboarding Result ===");
Console.WriteLine($"Decision: {result.Decision}");

if (result.Context?.Data.TryGetValue("kyc_result", out var kycObj)
    && kycObj is Dictionary<string, object> kycResult)
{
    Console.WriteLine($"KYC: {(kycResult.GetValueOrDefault("passed") is true ? "PASSED" : "FAILED")}");
}

Console.WriteLine();
Console.WriteLine("=== Full Trace ===");
foreach (var t in result.Trace)
    Console.WriteLine($"  {t.Timestamp:HH:mm:ss}  {t.ExpertId,-18} {t.DecisionCode,-12} {t.RuleFired}");

using UDEO.Pipeline;

namespace UDEO.Pipeline.Templates;

/// <summary>
/// Pre-built loan approval pipeline with 6 steps:
/// validation → validation → DTI → LTV → risk → human review.
/// Replaces the PowerShell New-UDEOLoanApprovalPipeline function.
/// </summary>
public static class LoanApprovalPipeline
{
    /// <summary>
    /// Creates a complete loan approval pipeline with the given applicant data.
    /// </summary>
    public static UdeoPipeline Create(
        double income = 75000,
        double debt = 25000,
        int creditScore = 720,
        double loanAmount = 300000,
        double interestRate = 0.065,
        int termMonths = 360,
        double propertyValue = 375000)
    {
        var pipeline = new UdeoPipeline("LoanApproval");

        // Seed context data
        pipeline.Context.Data["applicant"] = new Dictionary<string, object?>
        {
            ["name"] = "Applicant",
            ["income"] = income,
            ["debt"] = debt,
            ["credit_score"] = creditScore,
            ["loan_amount"] = loanAmount
        };
        pipeline.Context.Data["monthly_income"] = Math.Round(income / 12, 2);
        pipeline.Context.Data["monthly_debt"] = Math.Round(debt / 12, 2);
        pipeline.Context.Data["loan_amount"] = loanAmount;
        pipeline.Context.Data["property_value"] = propertyValue;
        pipeline.Context.Data["interest_rate"] = interestRate;
        pipeline.Context.Data["term_months"] = termMonths;

        // Step 1: Validate credit score
        pipeline.AddStep("udeo.validation", new Dictionary<string, object?>
        {
            ["Field"] = "credit_score",
            ["Schema"] = "credit_score",
            ["Required"] = true
        });

        // Step 2: Validate loan amount is positive
        pipeline.AddStep("udeo.validation", new Dictionary<string, object?>
        {
            ["Field"] = "loan_amount",
            ["Schema"] = "positive_number",
            ["Required"] = true
        });

        // Step 3: Calculate DTI
        pipeline.AddStep("udeo.math", new Dictionary<string, object?>
        {
            ["Operation"] = "dti"
        });

        // Step 4: Calculate LTV (if property value present)
        pipeline.AddConditionalStep("udeo.math",
            new Dictionary<string, object?> { ["Operation"] = "ltv" },
            ctx => !(ctx.Data.TryGetValue("property_value", out var pv)
                     && UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(pv) > 0));

        // Step 5: Risk assessment
        pipeline.AddStep("udeo.risk", new Dictionary<string, object?>
        {
            ["Rules"] = new List<Dictionary<string, object?>>
            {
                new() { ["Field"] = "credit_score", ["Op"] = "lt", ["Value"] = 640, ["Action"] = "REJECTED", ["Reason"] = "Credit score below 640" },
                new() { ["Field"] = "dti", ["Op"] = "gt", ["Value"] = 50, ["Action"] = "ROUTE_TO_HUMAN", ["Reason"] = "DTI exceeds 50%" },
                new() { ["Field"] = "ltv", ["Op"] = "gt", ["Value"] = 95, ["Action"] = "FLAGGED", ["Reason"] = "LTV exceeds 95%" }
            }
        });

        // Step 6: Human review (on continue failure)
        pipeline.AddStep("udeo.human", new Dictionary<string, object?>
        {
            ["Reason"] = "Risk assessment flagged for review"
        }, onFailure: FailurePolicy.Continue);

        return pipeline;
    }
}

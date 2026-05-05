using System.Data;
using UDEO.Core;
using UDEO.Core.Models;

namespace UDEO.Experts.BuiltIn;

/// <summary>
/// Mathematical computation expert: DTI, LTV, payment calculations.
/// Replaces the PowerShell udeo.math expert.
/// </summary>
public static class MathExpert
{
    public const string Id = "udeo.math";
    public const string Name = "Math Calculator";

    public static void Register()
    {
        var contract = new ExpertContract(Id, Name, UdeoExpertType.Math, Execute)
        {
            Description = "Performs financial math: DTI, LTV, payment calculations.",
            TimeoutSeconds = 10,
            HealthCheck = () => true
        };
        ExpertRegistry.Instance.Register(contract);
    }

    private static ExpertResult Execute(ExecutionContext context, Dictionary<string, object?> parameters)
    {
        var operation = parameters.GetValueOrDefault("Operation")?.ToString()?.ToLowerInvariant();
        var formula = parameters.GetValueOrDefault("Formula")?.ToString();

        EnsureCalculations(context);

        return operation switch
        {
            "dti" => CalculateDti(context),
            "ltv" => CalculateLtv(context),
            "payment" => CalculatePayment(context),
            _ => formula != null ? EvaluateFormula(context, operation ?? "custom", formula)
                 : ExpertResult.FailureResult($"Unknown operation: {operation}. Use dti, ltv, payment, or provide Formula.")
        };
    }

    private static void EnsureCalculations(ExecutionContext context)
    {
        if (!context.Data.ContainsKey("calculations"))
            context.Data["calculations"] = new Dictionary<string, object>();
    }

    private static ExpertResult CalculateDti(ExecutionContext context)
    {
        var income = context.Data.TryGetValue("monthly_income", out var mi)
            ? UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(mi) ?? 0
            : 0;
        var debt = context.Data.TryGetValue("monthly_debt", out var md)
            ? UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(md) ?? 0
            : 0;

        if (income <= 0)
            return ExpertResult.FailureResult("Monthly income must be > 0");

        var dti = Math.Round(debt / income * 100, 2);
        var calcs = (Dictionary<string, object>)context.Data["calculations"]!;
        calcs["dti"] = dti;
        calcs["dti_category"] = dti <= 36 ? "low" : dti <= 43 ? "moderate" : "high";

        return ExpertResult.SuccessResult("VALID", $"DTI_CALCULATED:{dti}%");
    }

    private static ExpertResult CalculateLtv(ExecutionContext context)
    {
        var loan = context.Data.TryGetValue("loan_amount", out var la)
            ? UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(la) ?? 0
            : 0;
        var propertyValue = context.Data.TryGetValue("property_value", out var pv)
            ? UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(pv) ?? 0
            : 0;

        if (propertyValue <= 0)
            return ExpertResult.FailureResult("Property value must be > 0");

        var ltv = Math.Round(loan / propertyValue * 100, 2);
        var calcs = (Dictionary<string, object>)context.Data["calculations"]!;
        calcs["ltv"] = ltv;

        return ExpertResult.SuccessResult("VALID", $"LTV_CALCULATED:{ltv}%");
    }

    private static ExpertResult CalculatePayment(ExecutionContext context)
    {
        var principal = context.Data.TryGetValue("loan_amount", out var la)
            ? UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(la) ?? 0
            : 0;
        var annualRate = context.Data.TryGetValue("interest_rate", out var ir)
            ? UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(ir) ?? 0
            : 0;
        var months = context.Data.TryGetValue("term_months", out var tm)
            ? UDEO.Core.Extensions.UdeoExtensions.ToIntSafe(tm) ?? 0
            : 0;

        if (months <= 0)
            return ExpertResult.FailureResult("Term months must be > 0");

        var monthlyRate = annualRate / 12.0;
        double payment;

        if (Math.Abs(monthlyRate) < 1e-10)
        {
            payment = principal / months;
        }
        else
        {
            var factor = Math.Pow(1 + monthlyRate, months);
            payment = principal * (monthlyRate * factor) / (factor - 1);
        }

        payment = Math.Round(payment, 2);
        var calcs = (Dictionary<string, object>)context.Data["calculations"]!;
        calcs["monthly_payment"] = payment;

        return ExpertResult.SuccessResult("VALID", $"PAYMENT_CALCULATED:{payment}");
    }

    private static ExpertResult EvaluateFormula(ExecutionContext context, string operation, string formula)
    {
        try
        {
            // Simple expression evaluation using DataTable.Compute (safe subset)
            var dt = new DataTable();
            var result = dt.Compute(formula.Replace(" ", ""), "");
            var calcs = (Dictionary<string, object>)context.Data["calculations"]!;
            calcs[operation] = Convert.ToDouble(result);

            return ExpertResult.SuccessResult("VALID", "FORMULA_EVALUATED");
        }
        catch (Exception ex)
        {
            return ExpertResult.FailureResult($"Formula error: {ex.Message}");
        }
    }
}

using UDEO.Core;
using UDEO.Core.Logging;
using UDEO.Core.Models;

namespace UDEO.Experts.BuiltIn;

/// <summary>
/// Risk assessment expert with configurable rules.
/// Replaces the PowerShell udeo.risk expert.
/// </summary>
public static class RiskExpert
{
    public const string Id = "udeo.risk";
    public const string Name = "Risk Assessor";

    public static void Register()
    {
        var contract = new ExpertContract(Id, Name, UdeoExpertType.Risk, Execute)
        {
            Description = "Assesses risk using configurable rules.",
            TimeoutSeconds = 15,
            HealthCheck = () => true
        };
        ExpertRegistry.Instance.Register(contract);
    }

    private static ExpertResult Execute(ExecutionContext context, Dictionary<string, object?> parameters)
    {
        var rules = ParseRules(parameters);

        foreach (var rule in rules)
        {
            var value = ResolveFieldValue(context, rule.Field);
            if (value == null) continue;

            var dblValue = UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(value);
            var dblThreshold = UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(rule.Value);

            bool triggered = rule.Op switch
            {
                "lt" => dblValue.HasValue && dblThreshold.HasValue && dblValue.Value < dblThreshold.Value,
                "lte" => dblValue.HasValue && dblThreshold.HasValue && dblValue.Value <= dblThreshold.Value,
                "gt" => dblValue.HasValue && dblThreshold.HasValue && dblValue.Value > dblThreshold.Value,
                "gte" => dblValue.HasValue && dblThreshold.HasValue && dblValue.Value >= dblThreshold.Value,
                "eq" => string.Equals(value?.ToString(), rule.Value?.ToString(), StringComparison.OrdinalIgnoreCase),
                "ne" => !string.Equals(value?.ToString(), rule.Value?.ToString(), StringComparison.OrdinalIgnoreCase),
                _ => false
            };

            if (triggered)
            {
                var action = rule.Action ?? "FLAGGED";
                var reason = rule.Reason ?? $"Rule triggered: {rule.Field} {rule.Op} {rule.Value}";
                UdeoLogger.Instance.Warn($"Risk rule triggered: {reason}", new { action });
                return ExpertResult.SuccessResult(action, reason);
            }
        }

        return ExpertResult.SuccessResult("APPROVED", "ALL_RULES_PASSED");
    }

    private static object? ResolveFieldValue(ExecutionContext context, string? fieldPath)
    {
        if (string.IsNullOrEmpty(fieldPath)) return null;

        // Try direct access
        if (context.Data.TryGetValue(fieldPath, out var direct))
            return direct;

        // Try calculations dict
        if (context.Data.TryGetValue("calculations", out var calcsObj)
            && calcsObj is Dictionary<string, object> calcs
            && calcs.TryGetValue(fieldPath, out var calcVal))
            return calcVal;

        // Try applicant dict
        if (context.Data.TryGetValue("applicant", out var appObj)
            && appObj is Dictionary<string, object> applicant
            && applicant.TryGetValue(fieldPath, out var appVal))
            return appVal;

        return null;
    }

    private static List<RiskRule> ParseRules(Dictionary<string, object?> parameters)
    {
        if (parameters.TryGetValue("Rules", out var rulesObj))
        {
            if (rulesObj is List<RiskRule> typedRules)
                return typedRules;

            if (rulesObj is List<Dictionary<string, object?>> rawRules)
            {
                return rawRules.Select(r => new RiskRule
                {
                    Field = r.GetValueOrDefault("Field")?.ToString() ?? string.Empty,
                    Op = r.GetValueOrDefault("Op")?.ToString() ?? "lt",
                    Value = r.GetValueOrDefault("Value"),
                    Action = r.GetValueOrDefault("Action")?.ToString() ?? "FLAGGED",
                    Reason = r.GetValueOrDefault("Reason")?.ToString() ?? string.Empty
                }).ToList();
            }
        }

        // Default rules
        return new List<RiskRule>
        {
            new() { Field = "credit_score", Op = "lt", Value = 640, Action = "REJECTED", Reason = "Credit score too low" },
            new() { Field = "dti", Op = "gt", Value = 50, Action = "ROUTE_TO_HUMAN", Reason = "High DTI ratio" },
            new() { Field = "ltv", Op = "gt", Value = 95, Action = "REJECTED", Reason = "LTV too high" }
        };
    }
}

public sealed class RiskRule
{
    public string Field { get; set; } = string.Empty;
    public string Op { get; set; } = "lt";
    public object? Value { get; set; }
    public string Action { get; set; } = "FLAGGED";
    public string Reason { get; set; } = string.Empty;
}

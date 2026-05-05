using UDEO.Core;
using UDEO.Core.Models;

namespace UDEO.Experts.BuiltIn;

/// <summary>
/// Schema-based validation expert.
/// Validates fields against schemas: positive_number, non_empty_string, credit_score.
/// Replaces the PowerShell udeo.validation expert.
/// </summary>
public static class ValidationExpert
{
    public const string Id = "udeo.validation";
    public const string Name = "Schema Validator";

    public static void Register()
    {
        var contract = new ExpertContract(Id, Name, UdeoExpertType.Validation, Execute)
        {
            Description = "Validates fields against defined schemas.",
            TimeoutSeconds = 10,
            HealthCheck = () => true
        };
        ExpertRegistry.Instance.Register(contract);
    }

    private static ExpertResult Execute(ExecutionContext context, Dictionary<string, object?> parameters)
    {
        var field = parameters.GetValueOrDefault("Field")?.ToString();
        var schema = parameters.GetValueOrDefault("Schema")?.ToString();
        var required = parameters.GetValueOrDefault("Required") as bool? ?? false;

        if (string.IsNullOrEmpty(field))
            return ExpertResult.FailureResult("Validation expert requires Field parameter");

        object? value = NavigateField(context.Data.ToDictionary(), field);

        if (value == null)
        {
            if (required)
                return ExpertResult.SuccessResult("INVALID", $"REQUIRED_FIELD_MISSING:{field}");
            return ExpertResult.SuccessResult("VALID", $"FIELD_OPTIONAL_MISSING:{field}");
        }

        if (string.IsNullOrEmpty(schema))
            return ExpertResult.SuccessResult("VALID", $"FIELD_PRESENT:{field}");

        return schema switch
        {
            "positive_number" => ValidatePositiveNumber(value, field),
            "non_empty_string" => ValidateNonEmptyString(value, field),
            "credit_score" => ValidateCreditScore(value, field),
            _ => ExpertResult.SuccessResult("VALID", $"SCHEMA_UNKNOWN_SKIPPED:{field}")
        };
    }

    private static object? NavigateField(Dictionary<string, object?> data, string fieldPath)
    {
        var parts = fieldPath.Split('.');
        object? current = data;

        foreach (var part in parts)
        {
            if (current is Dictionary<string, object?> dict && dict.TryGetValue(part, out var next))
            {
                current = next;
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    private static ExpertResult ValidatePositiveNumber(object? value, string field)
    {
        var d = UDEO.Core.Extensions.UdeoExtensions.ToDoubleSafe(value);
        if (d.HasValue && d.Value > 0)
            return ExpertResult.SuccessResult("VALID", $"SCHEMA_POSITIVE_NUMBER:{field}");
        return ExpertResult.SuccessResult("INVALID", $"SCHEMA_NOT_POSITIVE:{field}");
    }

    private static ExpertResult ValidateNonEmptyString(object? value, string field)
    {
        var s = value?.ToString()?.Trim();
        if (!string.IsNullOrEmpty(s))
            return ExpertResult.SuccessResult("VALID", $"SCHEMA_NON_EMPTY:{field}");
        return ExpertResult.SuccessResult("INVALID", $"SCHEMA_EMPTY_STRING:{field}");
    }

    private static ExpertResult ValidateCreditScore(object? value, string field)
    {
        var d = UDEO.Core.Extensions.UdeoExtensions.ToIntSafe(value);
        if (d.HasValue && d.Value >= 300 && d.Value <= 850)
            return ExpertResult.SuccessResult("VALID", $"SCHEMA_CREDIT_SCORE_RANGE:{field}");
        return ExpertResult.SuccessResult("INVALID", $"SCHEMA_CREDIT_SCORE_OUT_OF_RANGE:{field}");
    }
}

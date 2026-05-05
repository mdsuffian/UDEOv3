using UDEO.Core;
using UDEO.Core.Configuration;
using UDEO.Core.Logging;
using UDEO.Core.Models;

namespace UDEO.Experts.BuiltIn;

/// <summary>
/// Human review escalation expert.
/// Replaces the PowerShell udeo.human expert.
/// </summary>
public static class HumanReviewExpert
{
    public const string Id = "udeo.human";
    public const string Name = "Human Reviewer";

    public static void Register()
    {
        var contract = new ExpertContract(Id, Name, UdeoExpertType.HumanReview, Execute)
        {
            Description = "Escalates decisions requiring human review.",
            TimeoutSeconds = 60,
            HealthCheck = () => true
        };
        ExpertRegistry.Instance.Register(contract);
    }

    private static ExpertResult Execute(ExecutionContext context, Dictionary<string, object?> parameters)
    {
        var reason = parameters.GetValueOrDefault("Reason")?.ToString() ?? "Manual review required";
        var timeoutSeconds = parameters.GetValueOrDefault("TimeoutSeconds") is int ts ? ts : 30;

        UdeoLogger.Instance.Warn($"Human review required: {reason}");
        UdeoLogger.Instance.Info($"Waiting {timeoutSeconds} seconds for human input...");

        context.Data.TryAdd("human_review", new Dictionary<string, object>());
        var reviewData = (Dictionary<string, object>)context.Data["human_review"]!;

        reviewData["reason"] = reason;
        reviewData["timestamp"] = DateTime.UtcNow.ToString("O");

        // Auto-escalate if configured
        if (UdeoConfig.Instance.GetBool("pipeline.autoEscalate", true))
        {
            reviewData["decision"] = "ESCALATED";
            reviewData["status"] = "AUTO_ESCALATED";
            return ExpertResult.SuccessResult("ROUTE_TO_HUMAN", "AUTO_ESCALATED");
        }

        reviewData["status"] = "PENDING_REVIEW";
        return ExpertResult.SuccessResult("ROUTE_TO_HUMAN", "HUMAN_REVIEW_NEEDED");
    }
}

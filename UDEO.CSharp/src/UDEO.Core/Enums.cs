namespace UDEO.Core;

/// <summary>
/// Expert type classification.
/// </summary>
public enum UdeoExpertType
{
    Rule,
    Math,
    Validation,
    Risk,
    HumanReview,
    Custom
}

/// <summary>
/// Decision codes produced by experts.
/// </summary>
public enum UdeoDecisionCode
{
    Approved,
    Rejected,
    Flagged,
    RouteToHuman,
    Valid,
    Invalid,
    Pending,
    Error
}

/// <summary>
/// Pipeline lifecycle status.
/// </summary>
public enum UdeoPipelineStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

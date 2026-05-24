namespace Ums.Application.IGA.PromotionRequest.Commands;

/// <summary>
/// Records the security team's risk assessment.
/// When <see cref="IsHighRisk"/> is false, the request moves directly to ApprovedReadyToExecute.
/// When true, it escalates to PendingSecurityApproval for a second explicit decision.
/// </summary>
public sealed record SecurityReviewPromotionRequestCommand(Guid PromotionRequestId, bool IsHighRisk) : ICommand;

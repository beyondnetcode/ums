namespace Ums.Domain.Enums;

public enum PromotionStatus
{
    Draft = 1,
    PendingManagerApproval = 2,
    PendingSecurityReview = 3,
    PendingSecurityApproval = 4,
    ApprovedReadyToExecute = 5,
    Executed = 6,
    Verified = 7,
    Rejected = 8,
    VerificationFailed = 9
}

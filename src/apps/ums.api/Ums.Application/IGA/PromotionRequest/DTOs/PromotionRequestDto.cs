namespace Ums.Application.IGA.PromotionRequest.DTOs;

public sealed record PromotionRequestDto(
    Guid PromotionRequestId,
    Guid TenantId,
    Guid UserId,
    Guid CurrentRoleId,
    Guid TargetRoleId,
    DateTime RequestedAt,
    string ManagerApprovalStatus,
    string SecurityApprovalStatus,
    string Status,
    DateTime? ExecutedAt);

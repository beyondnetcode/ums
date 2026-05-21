using Ums.Application.IGA.PromotionRequest.DTOs;

namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record CreatePromotionRequestCommand(
    Guid TenantId, Guid UserId, Guid CurrentRoleId, Guid TargetRoleId,
    Guid ManagerId, string? RequestReason) : ICommand<CreatePromotionRequestResponse>;

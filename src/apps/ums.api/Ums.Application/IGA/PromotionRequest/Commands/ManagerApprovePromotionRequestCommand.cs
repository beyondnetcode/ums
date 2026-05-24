namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record ManagerApprovePromotionRequestCommand(Guid PromotionRequestId) : ICommand;

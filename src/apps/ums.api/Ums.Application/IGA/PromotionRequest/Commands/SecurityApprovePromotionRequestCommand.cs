namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record SecurityApprovePromotionRequestCommand(Guid PromotionRequestId) : ICommand;

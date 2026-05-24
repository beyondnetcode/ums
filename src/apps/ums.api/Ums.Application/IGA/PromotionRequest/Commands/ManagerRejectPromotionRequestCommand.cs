namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record ManagerRejectPromotionRequestCommand(Guid PromotionRequestId) : ICommand;

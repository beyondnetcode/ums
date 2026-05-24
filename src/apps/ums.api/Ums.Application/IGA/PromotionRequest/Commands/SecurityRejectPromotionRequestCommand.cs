namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record SecurityRejectPromotionRequestCommand(Guid PromotionRequestId) : ICommand;

namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record VerifyPromotionRequestCommand(Guid PromotionRequestId) : ICommand;

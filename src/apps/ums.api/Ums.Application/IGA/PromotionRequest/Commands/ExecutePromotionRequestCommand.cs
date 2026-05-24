namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record ExecutePromotionRequestCommand(Guid PromotionRequestId) : ICommand;

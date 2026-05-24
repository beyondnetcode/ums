namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record MarkVerificationFailedCommand(Guid PromotionRequestId) : ICommand;

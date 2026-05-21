using Ums.Application.IGA.PromotionRequest.DTOs;

namespace Ums.Application.IGA.PromotionRequest.Commands;

public sealed record SubmitPromotionRequestCommand(Guid PromotionRequestId) : ICommand;

using Ums.Application.IGA.PromotionRequest.DTOs;

namespace Ums.Application.IGA.PromotionRequest.Queries;

public sealed record GetPromotionRequestByIdQuery(Guid PromotionRequestId) : IQuery<PromotionRequestDto>;

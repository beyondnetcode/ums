using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Domain.IGA;
using Ums.Domain.IGA.PromotionRequest;

namespace Ums.Application.IGA.PromotionRequest.Queries;

public sealed class GetPromotionRequestByIdQueryHandler : IQueryHandler<GetPromotionRequestByIdQuery, PromotionRequestDto>
{
    private readonly IPromotionRequestRepository _repository;

    public GetPromotionRequestByIdQueryHandler(IPromotionRequestRepository repository) => _repository = repository;

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PromotionRequestDto>> Handle(GetPromotionRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.PromotionRequestId, cancellationToken);
        if (entity is null) return Result<PromotionRequestDto>.Failure("Promotion request not found.");

        return Result<PromotionRequestDto>.Success(new PromotionRequestDto(
            entity.Props.Id.GetValue(), entity.Props.TenantId.GetValue(), entity.Props.UserId.GetValue(),
            entity.Props.CurrentRoleId.GetValue(), entity.Props.TargetRoleId.GetValue(), entity.Props.RequestedAt,
            entity.Props.ManagerApprovalStatus.ToString(), entity.Props.SecurityApprovalStatus.ToString(),
            entity.Props.Status.ToString(), entity.Props.ExecutedAt));
    }
}

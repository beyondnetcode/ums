using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Domain.IGA;
using Ums.Domain.IGA.RoleMaturityStatus;

namespace Ums.Application.IGA.RoleMaturityStatus.Queries;

public sealed class GetRoleMaturityStatusByIdQueryHandler : IQueryHandler<GetRoleMaturityStatusByIdQuery, RoleMaturityStatusDto>
{
    private readonly IRoleMaturityStatusRepository _repository;

    public GetRoleMaturityStatusByIdQueryHandler(IRoleMaturityStatusRepository repository) => _repository = repository;

    public async Task<Result<RoleMaturityStatusDto>> Handle(GetRoleMaturityStatusByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.RoleMaturityStatusId, cancellationToken);
        if (entity is null) return Result<RoleMaturityStatusDto>.Failure("Role maturity status not found.");

        return Result<RoleMaturityStatusDto>.Success(new RoleMaturityStatusDto(
            entity.Props.Id.GetValue(), entity.Props.TenantId.GetValue(), entity.Props.UserId.GetValue(),
            entity.Props.RoleId.GetValue(), entity.Props.CurrentMaturityLevel.ToString(),
            entity.Props.NextEligibleMaturityLevel?.ToString(), entity.Props.AssignedAt, entity.Props.CurrentLevelSince,
            entity.Props.EligibleForPromotionAt, entity.Props.CompletedCertificationsCount, entity.Props.CompletedTrainingsCount,
            entity.Props.PerformanceScore, entity.Props.HasNoComplianceIssues, entity.Props.BlockingFactor?.GetValue(),
            entity.Props.LastReviewedAt));
    }
}

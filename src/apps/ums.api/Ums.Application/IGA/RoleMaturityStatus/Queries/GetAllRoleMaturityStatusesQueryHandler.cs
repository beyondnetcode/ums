using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Domain.IGA;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.IGA.RoleMaturityStatus.Queries;

public sealed class GetAllRoleMaturityStatusesQueryHandler : IQueryHandler<GetAllRoleMaturityStatusesQuery, PagedResult<RoleMaturityStatusDto>>
{
    private readonly IRoleMaturityStatusRepository _repository;

    public GetAllRoleMaturityStatusesQueryHandler(IRoleMaturityStatusRepository repository) => _repository = repository;

    public async Task<Result<PagedResult<RoleMaturityStatusDto>>> Handle(GetAllRoleMaturityStatusesQuery request, CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var sortBy = NormalizeText(request.SortBy, "level").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();

        var items = request.UserId.HasValue
            ? await _repository.GetByUserIdAsync(request.UserId.Value, cancellationToken)
            : request.TenantId.HasValue
                ? await _repository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
                : await _repository.GetAllAsync(cancellationToken);

        var query = items.Select(r => new RoleMaturityStatusDto(
            r.Props.Id.GetValue(), r.Props.TenantId.GetValue(), r.Props.UserId.GetValue(),
            r.Props.RoleId.GetValue(), r.Props.CurrentMaturityLevel.ToString(),
            r.Props.NextEligibleMaturityLevel?.ToString(), r.Props.AssignedAt, r.Props.CurrentLevelSince,
            r.Props.EligibleForPromotionAt, r.Props.CompletedCertificationsCount, r.Props.CompletedTrainingsCount,
            r.Props.PerformanceScore, r.Props.HasNoComplianceIssues, r.Props.BlockingFactor?.GetValue(),
            r.Props.LastReviewedAt));

        query = (sortBy, sortOrder) switch
        {
            ("level", "desc") => query.OrderByDescending(r => r.CurrentMaturityLevel),
            ("performance", "desc") => query.OrderByDescending(r => r.PerformanceScore),
            ("assignedat", "desc") => query.OrderByDescending(r => r.AssignedAt),
            _ => query.OrderBy(r => r.CurrentMaturityLevel),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<RoleMaturityStatusDto>>.Success(new PagedResult<RoleMaturityStatusDto>(paged, page, pageSize, totalItems, totalPages));
    }
}

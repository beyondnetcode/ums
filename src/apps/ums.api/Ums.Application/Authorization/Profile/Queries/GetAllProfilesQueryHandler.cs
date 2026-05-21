using Ums.Application.Authorization.Profile.DTOs;
using Ums.Domain.Authorization;

namespace Ums.Application.Authorization.Profile.Queries;

public sealed class GetAllProfilesQueryHandler : IQueryHandler<GetAllProfilesQuery, PagedResult<ProfileDto>>
{
    private readonly IProfileRepository _profileRepository;

    public GetAllProfilesQueryHandler(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<Result<PagedResult<ProfileDto>>> Handle(
        GetAllProfilesQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var criteria = request.Criteria.Trim().ToLowerInvariant();
        var status = request.Status.Trim();
        var sortBy = request.SortBy.Trim().ToLowerInvariant();
        var sortOrder = request.SortOrder.Trim().ToLowerInvariant();
        var search = request.Search?.Trim();

        var profiles = request.UserId.HasValue
            ? await _profileRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken)
            : request.TenantId.HasValue
                ? await _profileRepository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
                : await _profileRepository.GetAllAsync(cancellationToken);

        var query = profiles.Select(p => new ProfileDto(
            p.Props.Id.GetValue(),
            p.Props.TenantId.GetValue(),
            p.Props.UserId.GetValue(),
            p.Props.RoleId.GetValue(),
            p.Props.BranchId?.GetValue(),
            p.Props.Scope.ToString(),
            p.Props.IsActive));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var isActiveStatus = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(p => p.IsActive == isActiveStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = criteria switch
            {
                "id" => query.Where(p => p.ProfileId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)),
                "roleid" => query.Where(p => p.RoleId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)),
                _ => query.Where(p => p.UserId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)),
            };
        }

        query = (sortBy, sortOrder) switch
        {
            ("scope", "desc") => query.OrderByDescending(p => p.Scope),
            ("scope", _) => query.OrderBy(p => p.Scope),
            ("roleid", "desc") => query.OrderByDescending(p => p.RoleId),
            ("roleid", _) => query.OrderBy(p => p.RoleId),
            _ => query.OrderBy(p => p.UserId),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<PagedResult<ProfileDto>>.Success(new PagedResult<ProfileDto>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages));
    }
}

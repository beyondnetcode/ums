using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Domain.Identity;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Identity.UserAccount.Queries;

public sealed class GetAllUserAccountsQueryHandler : IQueryHandler<GetAllUserAccountsQuery, PagedResult<UserAccountDto>>
{
    private readonly IUserAccountRepository _userAccountRepository;

    public GetAllUserAccountsQueryHandler(IUserAccountRepository userAccountRepository)
    {
        _userAccountRepository = userAccountRepository;
    }

    public async Task<Result<PagedResult<UserAccountDto>>> Handle(
        GetAllUserAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var criteria = NormalizeText(request.Criteria, "email").ToLowerInvariant();
        var status = NormalizeText(request.Status, "all");
        var sortBy = NormalizeText(request.SortBy, "email").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();
        var search = NormalizeSearch(request.Search);

        var userAccounts = request.TenantId.HasValue
            ? await _userAccountRepository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
            : await _userAccountRepository.GetAllAsync(cancellationToken);

        var query = userAccounts.Select(u => new UserAccountDto(
            u.Props.Id.GetValue(),
            u.Props.TenantId.GetValue(),
            u.Props.BranchId?.GetValue(),
            u.Props.Email.GetValue(),
            u.Props.Category.ToString(),
            u.Props.Status.ToString(),
            u.Props.IdentityReference?.GetValue(),
            u.Props.IdentityReferenceType?.ToString()));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u => string.Equals(u.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = criteria switch
            {
                "id" => query.Where(u => u.UserAccountId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)),
                _ => query.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)),
            };
        }

        query = (sortBy, sortOrder) switch
        {
            ("status", "desc") => query.OrderByDescending(u => u.Status),
            ("status", _) => query.OrderBy(u => u.Status),
            ("category", "desc") => query.OrderByDescending(u => u.Category),
            ("category", _) => query.OrderBy(u => u.Category),
            _ => query.OrderBy(u => u.Email),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<PagedResult<UserAccountDto>>.Success(new PagedResult<UserAccountDto>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages));
    }
}

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

        // REC-12: Use GetPagedAsync so SQL implementations push Skip/Take to the DB.
        var (userAccounts, totalItems) = await _userAccountRepository.GetPagedAsync(
            page, pageSize, search, status, sortBy, sortOrder,
            tenantId: request.TenantId,
            cancellationToken: cancellationToken);

        var items = userAccounts.Select(u => new UserAccountDto(
            u.Props.Id.GetValue(),
            u.Props.TenantId.GetValue(),
            u.Props.BranchId?.GetValue(),
            u.Props.Email.GetValue(),
            u.Props.Category.ToString(),
            u.Props.Status.ToString(),
            u.Props.IdentityReference?.GetValue(),
            u.Props.IdentityReferenceType?.ToString()))
            .ToList();

        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return Result<PagedResult<UserAccountDto>>.Success(new PagedResult<UserAccountDto>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages));
    }
}

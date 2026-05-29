using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Domain.Identity;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Identity.UserAccount.Queries;

public sealed class GetAllUserAccountsQueryHandler : IQueryHandler<GetAllUserAccountsQuery, PagedResult<UserAccountDto>>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserContext _userContext;

    public GetAllUserAccountsQueryHandler(IUserAccountRepository userAccountRepository, IUserContext userContext)
    {
        _userAccountRepository = userAccountRepository;
        _userContext = userContext;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

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

        var effectiveTenantId = request.TenantId ?? (
            !string.IsNullOrWhiteSpace(_userContext.TenantId) && Guid.TryParse(_userContext.TenantId, out var ctxTenantId)
                ? ctxTenantId
                : (Guid?)null);

        var (userAccounts, totalItems) = await _userAccountRepository.GetPagedAsync(
            page, pageSize, search, status, sortBy, sortOrder,
            tenantId: effectiveTenantId,
            cancellationToken: cancellationToken);

        var items = userAccounts.Select(u =>
        {
            var activePassword = u.PasswordCredentials.SingleOrDefault(x => x.IsActive);
            return new UserAccountDto(
                u.Props.Id.GetValue(),
                u.Props.TenantId.GetValue(),
                u.Props.BranchId?.GetValue(),
                u.Props.Email.GetValue(),
                u.Props.Category.ToString(),
                u.Props.Status.ToString(),
                u.Props.IdentityReference?.GetValue(),
                u.Props.IdentityReferenceType?.ToString(),
                activePassword is not null,
                activePassword?.Props.Audit.GetValue().UpdatedAt ?? activePassword?.Props.Audit.GetValue().CreatedAt);
        })
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

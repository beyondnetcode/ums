using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Domain.IGA;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.IGA.PromotionRequest.Queries;

public sealed class GetAllPromotionRequestsQueryHandler : IQueryHandler<GetAllPromotionRequestsQuery, PagedResult<PromotionRequestDto>>
{
    private readonly IPromotionRequestRepository _repository;

    public GetAllPromotionRequestsQueryHandler(IPromotionRequestRepository repository) => _repository = repository;

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PagedResult<PromotionRequestDto>>> Handle(GetAllPromotionRequestsQuery request, CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var status = NormalizeText(request.Status, "all");
        var sortBy = NormalizeText(request.SortBy, "requestedat").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();

        var items = request.UserId.HasValue
            ? await _repository.GetByUserIdAsync(request.UserId.Value, cancellationToken)
            : request.TenantId.HasValue
                ? await _repository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
                : await _repository.GetAllAsync(request.TenantId, cancellationToken);

        var query = items.Select(r => new PromotionRequestDto(
            r.Props.Id.GetValue(), r.Props.TenantId.GetValue(), r.Props.UserId.GetValue(),
            r.Props.CurrentRoleId.GetValue(), r.Props.TargetRoleId.GetValue(), r.Props.RequestedAt,
            r.Props.ManagerApprovalStatus.ToString(), r.Props.SecurityApprovalStatus.ToString(),
            r.Props.Status.ToString(), r.Props.ExecutedAt));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));

        query = (sortBy, sortOrder) switch
        {
            ("requestedat", "desc") => query.OrderByDescending(r => r.RequestedAt),
            ("status", "desc") => query.OrderByDescending(r => r.Status),
            _ => query.OrderBy(r => r.RequestedAt),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<PromotionRequestDto>>.Success(new PagedResult<PromotionRequestDto>(paged, page, pageSize, totalItems, totalPages));
    }
}

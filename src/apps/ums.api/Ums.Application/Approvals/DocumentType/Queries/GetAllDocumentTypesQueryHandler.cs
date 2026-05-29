using Ums.Application.Approvals.DocumentType.DTOs;
using Ums.Domain.Approvals;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Approvals.DocumentType.Queries;

public sealed class GetAllDocumentTypesQueryHandler : IQueryHandler<GetAllDocumentTypesQuery, PagedResult<DocumentTypeDto>>
{
    private readonly IDocumentTypeRepository _repository;

    public GetAllDocumentTypesQueryHandler(IDocumentTypeRepository repository) => _repository = repository;

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PagedResult<DocumentTypeDto>>> Handle(GetAllDocumentTypesQuery request, CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var sortBy = NormalizeText(request.SortBy, "name").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();
        var search = NormalizeSearch(request.Search);

        var items = request.TenantId.HasValue
            ? await _repository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
            : await _repository.GetAllAsync(request.TenantId, cancellationToken);

        var query = items.Select(d => new DocumentTypeDto(
            d.Props.Id.GetValue(), d.Props.TenantId.GetValue(), d.Props.Code.GetValue(),
            d.Props.Name.GetValue(), d.Props.Description.GetValue(), d.Props.Criticity.ToString()));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        query = (sortBy, sortOrder) switch
        {
            ("code", "desc") => query.OrderByDescending(d => d.Code),
            ("criticity", "desc") => query.OrderByDescending(d => d.Criticity),
            _ => query.OrderBy(d => d.Name),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<DocumentTypeDto>>.Success(new PagedResult<DocumentTypeDto>(paged, page, pageSize, totalItems, totalPages));
    }
}

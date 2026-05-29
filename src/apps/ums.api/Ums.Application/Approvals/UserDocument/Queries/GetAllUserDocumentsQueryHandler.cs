using Ums.Application.Approvals.UserDocument.DTOs;
using Ums.Domain.Approvals;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Approvals.UserDocument.Queries;

public sealed class GetAllUserDocumentsQueryHandler : IQueryHandler<GetAllUserDocumentsQuery, PagedResult<UserDocumentDto>>
{
    private readonly IUserDocumentRepository _repository;

    public GetAllUserDocumentsQueryHandler(IUserDocumentRepository repository) => _repository = repository;

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PagedResult<UserDocumentDto>>> Handle(GetAllUserDocumentsQuery request, CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var status = NormalizeText(request.Status, "all");
        var sortBy = NormalizeText(request.SortBy, "expirationdate").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();
        var search = NormalizeSearch(request.Search);

        var items = request.UserId.HasValue
            ? await _repository.GetByUserIdAsync(request.UserId.Value, cancellationToken)
            : await _repository.GetAllAsync(null, cancellationToken);

        var query = items.Select(d => new UserDocumentDto(
            d.Props.Id.GetValue(), d.Props.UserId.GetValue(), d.Props.DocumentTypeId.GetValue(),
            d.Props.IssueDate, d.Props.ExpirationDate, d.Props.Status.ToString(),
            d.Props.Criticity.ToString(), d.Props.FileStoragePath.GetValue(),
            d.Props.FileChecksum, d.Props.NotificationStep));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(d => string.Equals(d.Status, status, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.FileStoragePath.Contains(search, StringComparison.OrdinalIgnoreCase));

        query = (sortBy, sortOrder) switch
        {
            ("status", "desc") => query.OrderByDescending(d => d.Status),
            ("expirationdate", "desc") => query.OrderByDescending(d => d.ExpirationDate),
            _ => query.OrderBy(d => d.ExpirationDate),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<UserDocumentDto>>.Success(new PagedResult<UserDocumentDto>(paged, page, pageSize, totalItems, totalPages));
    }
}

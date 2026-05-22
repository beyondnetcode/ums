using Ums.Application.Audit.AuditRecord.DTOs;
using Ums.Domain.Audit.AuditRecord;
using AuditRecordAggregate = Ums.Domain.Audit.AuditRecord.AuditRecord;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Audit.AuditRecord.Queries;

public sealed class GetAllAuditRecordsQueryHandler : IQueryHandler<GetAllAuditRecordsQuery, PagedResult<AuditRecordDto>>
{
    private readonly IAuditRecordRepository _auditRecordRepository;

    public GetAllAuditRecordsQueryHandler(IAuditRecordRepository auditRecordRepository)
    {
        _auditRecordRepository = auditRecordRepository;
    }

    public async Task<Result<PagedResult<AuditRecordDto>>> Handle(
        GetAllAuditRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var from = request.From ?? DateTime.UtcNow.AddDays(-30);
        var to = request.To ?? DateTime.UtcNow;

        IReadOnlyList<AuditRecordAggregate> records;

        if (request.ActorId.HasValue)
        {
            records = await _auditRecordRepository.QueryByActorAsync(request.ActorId.Value, from, to, cancellationToken);
        }
        else if (request.EntityId.HasValue && !string.IsNullOrWhiteSpace(request.EntityType))
        {
            records = await _auditRecordRepository.QueryByEntityAsync(request.EntityId.Value, request.EntityType, request.TenantId ?? Guid.Empty, from, to, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.EventType))
        {
            records = await _auditRecordRepository.QueryByEventTypeAsync(request.EventType, request.TenantId ?? Guid.Empty, from, to, cancellationToken);
        }
        else
        {
            records = await _auditRecordRepository.QueryByEventTypeAsync("*", request.TenantId ?? Guid.Empty, from, to, cancellationToken);
        }

        var query = records.Select(r => new AuditRecordDto(
            r.Props.Id.GetValue(),
            r.Props.WhoActed,
            r.Props.SubjectType.ToString(),
            r.Props.WhenOccurred,
            r.Props.WhatChanged,
            r.Props.EventType,
            r.Props.AuditResult.ToString(),
            r.Props.AffectedEntityId,
            r.Props.AffectedEntityType,
            r.Props.RootTenantId,
            r.Props.Metadata));

        if (!string.IsNullOrWhiteSpace(request.EventType) && request.EventType != "*")
        {
            query = query.Where(r => r.EventType.Equals(request.EventType, StringComparison.OrdinalIgnoreCase));
        }

        query = query.OrderByDescending(r => r.WhenOccurred);

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<AuditRecordDto>>.Success(new PagedResult<AuditRecordDto>(
            items, page, pageSize, totalItems, totalPages));
    }
}

using Ums.Application.Audit.AuditRecord.DTOs;
using Ums.Domain.Audit.AuditRecord;

namespace Ums.Application.Audit.AuditRecord.Queries;

public sealed class GetAuditRecordByIdQueryHandler : IQueryHandler<GetAuditRecordByIdQuery, AuditRecordDto>
{
    private readonly IAuditRecordRepository _auditRecordRepository;

    public GetAuditRecordByIdQueryHandler(IAuditRecordRepository auditRecordRepository)
    {
        _auditRecordRepository = auditRecordRepository;
    }

    public async Task<Result<AuditRecordDto>> Handle(
        GetAuditRecordByIdQuery request,
        CancellationToken cancellationToken)
    {
        var record = await _auditRecordRepository.GetByIdAsync(request.AuditRecordId, cancellationToken);

        if (record is null)
        {
            return Result<AuditRecordDto>.Failure("Audit record not found.");
        }

        return Result<AuditRecordDto>.Success(new AuditRecordDto(
            record.Props.Id.GetValue(),
            record.Props.WhoActed,
            record.Props.SubjectType.ToString(),
            record.Props.WhenOccurred,
            record.Props.WhatChanged,
            record.Props.EventType,
            record.Props.AuditResult.ToString(),
            record.Props.AffectedEntityId,
            record.Props.AffectedEntityType,
            record.Props.RootTenantId,
            record.Props.Metadata));
    }
}

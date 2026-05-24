using Ums.Application.Audit.AuditRecord.DTOs;
using Ums.Application.Common.Aop;
using Ums.Shell.Aop.Aspects;

namespace Ums.Application.Audit.AuditRecord.Commands;

using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Enums;

public sealed class RecordAuditCommandHandler : ICommandHandler<RecordAuditCommand, RecordAuditResponse>
{
    private readonly IAuditRecordRepository _auditRecordRepository;

    public RecordAuditCommandHandler(IAuditRecordRepository auditRecordRepository)
    {
        _auditRecordRepository = auditRecordRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<RecordAuditResponse>> Handle(
        RecordAuditCommand request,
        CancellationToken cancellationToken)
    {
        var subjectType = DomainEnumerationParser.FromName<SubjectType>(request.SubjectType) ?? SubjectType.User;
        var auditResult = DomainEnumerationParser.FromName<AuditResult>(request.AuditResult) ?? AuditResult.Success;

        var auditRecordResult = AuditRecord.Record(
            request.WhoActed,
            subjectType,
            request.WhatChanged,
            request.EventType,
            auditResult,
            request.AffectedEntityId,
            request.AffectedEntityType,
            request.RootTenantId,
            request.Metadata);

        if (auditRecordResult.IsFailure)
        {
            return Result<RecordAuditResponse>.Failure(auditRecordResult.Error);
        }

        await _auditRecordRepository.AppendAsync(auditRecordResult.Value, cancellationToken);
        await _auditRecordRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<RecordAuditResponse>.Success(
            new RecordAuditResponse(auditRecordResult.Value.Props.Id.GetValue()));
    }
}

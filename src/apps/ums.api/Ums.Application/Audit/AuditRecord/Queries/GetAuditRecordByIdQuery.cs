using Ums.Application.Audit.AuditRecord.DTOs;

namespace Ums.Application.Audit.AuditRecord.Queries;

public sealed record GetAuditRecordByIdQuery(Guid AuditRecordId) : IQuery<AuditRecordDto>;

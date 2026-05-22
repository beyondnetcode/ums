namespace Ums.Presentation.GraphQL.Audit;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Audit.AuditRecord.DTOs;
using Ums.Application.Audit.AuditRecord.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class AuditRecordQueries
{
    public async Task<PagedResult<AuditRecordDto>> GetAuditRecordsAsync(
        int page,
        int pageSize,
        string? eventType,
        Guid? actorId,
        Guid? entityId,
        string? entityType,
        Guid? tenantId,
        DateTime? from,
        DateTime? to,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAuditRecordsQuery(
            NormalizePage(page),
            NormalizePageSize(pageSize),
            eventType, actorId, entityId, entityType, tenantId, from, to), cancellationToken);

        return result.UnwrapGraphQl();
    }

    public async Task<AuditRecordDto?> GetAuditRecordByIdAsync(
        Guid auditRecordId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAuditRecordByIdQuery(auditRecordId), cancellationToken);

        return result.UnwrapGraphQlOrNull();
    }
}

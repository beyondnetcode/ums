namespace Ums.Presentation.GraphQL.Audit;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Audit.AuditRecord.DTOs;
using Ums.Application.Audit.AuditRecord.Queries;

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
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            eventType, actorId, entityId, entityType, tenantId, from, to), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    public async Task<AuditRecordDto?> GetAuditRecordByIdAsync(
        Guid auditRecordId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAuditRecordByIdQuery(auditRecordId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }

    private static GraphQLException BuildQueryException(string message) =>
        new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode("UMS_QUERY_ERROR")
            .Build());
}

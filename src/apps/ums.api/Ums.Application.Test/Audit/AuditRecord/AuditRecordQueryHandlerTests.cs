namespace Ums.Application.Test.Audit.AuditRecord;

using Ums.Application.Audit.AuditRecord.Queries;
using Ums.Application.Audit.AuditRecord.DTOs;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Enums;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class AuditRecordQueryHandlerTests
{
    private readonly Mock<IAuditRecordRepository> _repo = new();

    private static AuditRecord MakeAuditRecord()
    {
        return AuditRecord.Record(
            Guid.NewGuid(),
            SubjectType.User,
            "Updated user name",
            "UserUpdated",
            AuditResult.Success,
            Guid.NewGuid(),
            "UserAccount",
            Guid.NewGuid(),
            "{}").Value;
    }

    // =========================================================================
    #region GetAuditRecordByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_WhenFound_ReturnsSuccess()
    {
        var record = MakeAuditRecord();
        var recordId = record.Props.Id.GetValue();
        _repo.Setup(r => r.GetByIdAsync(recordId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(record);

        var query = new GetAuditRecordByIdQuery(recordId);
        var handler = new GetAuditRecordByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(recordId, result.Value.AuditRecordId);
        Assert.Equal("UserUpdated", result.Value.EventType);
        Assert.Equal("Success", result.Value.AuditResult);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((AuditRecord?)null);

        var query = new GetAuditRecordByIdQuery(Guid.NewGuid());
        var handler = new GetAuditRecordByIdQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("audit record not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    // =========================================================================
    #region GetAllAuditRecordsQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_WithActorFilter_ReturnsActorItems()
    {
        var actorId = Guid.NewGuid();
        var record = MakeAuditRecord();
        var list = new List<AuditRecord> { record };

        _repo.Setup(r => r.QueryByActorAsync(actorId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllAuditRecordsQuery(
            TenantId: null,
            ActorId: actorId,
            EntityId: null,
            EntityType: null,
            EventType: null,
            From: null,
            To: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllAuditRecordsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        _repo.Verify(r => r.QueryByActorAsync(actorId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithEntityFilter_ReturnsEntityItems()
    {
        var entityId = Guid.NewGuid();
        var entityType = "UserAccount";
        var tenantId = Guid.NewGuid();
        var record = MakeAuditRecord();
        var list = new List<AuditRecord> { record };

        _repo.Setup(r => r.QueryByEntityAsync(entityId, entityType, tenantId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllAuditRecordsQuery(
            TenantId: tenantId,
            ActorId: null,
            EntityId: entityId,
            EntityType: entityType,
            EventType: null,
            From: null,
            To: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllAuditRecordsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        _repo.Verify(r => r.QueryByEntityAsync(entityId, entityType, tenantId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithEventTypeFilter_ReturnsEventTypeItems()
    {
        var eventType = "UserUpdated";
        var tenantId = Guid.NewGuid();
        var record = MakeAuditRecord();
        var list = new List<AuditRecord> { record };

        _repo.Setup(r => r.QueryByEventTypeAsync(eventType, tenantId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var query = new GetAllAuditRecordsQuery(
            TenantId: tenantId,
            ActorId: null,
            EntityId: null,
            EntityType: null,
            EventType: eventType,
            From: null,
            To: null,
            Page: 1,
            PageSize: 10);

        var handler = new GetAllAuditRecordsQueryHandler(_repo.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalItems);
        _repo.Verify(r => r.QueryByEventTypeAsync(eventType, tenantId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}

namespace Ums.Application.Test.Audit.AuditRecord;

using Ums.Application.Common.Interfaces;
using Ums.Application.Audit.AuditRecord.Commands;
using Ums.Application.Audit.AuditRecord.DTOs;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Enums;
using Ums.Domain.Kernel;
using Ums.Shell.Ddd.Interfaces;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class AuditRecordCommandHandlerTests
{
    private readonly Mock<IAuditRecordRepository>             _repo = new();
    private readonly Mock<Ums.Shell.Ddd.Interfaces.IUnitOfWork> _uow  = new();

    public AuditRecordCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Record_WithValidCommand_ReturnsSuccess()
    {
        var cmd = new RecordAuditCommand(
            WhoActed: Guid.NewGuid(),
            SubjectType: "User",
            WhatChanged: "Updated user name",
            EventType: "UserUpdated",
            AuditResult: "Success",
            AffectedEntityId: Guid.NewGuid(),
            AffectedEntityType: "UserAccount",
            RootTenantId: Guid.NewGuid(),
            Metadata: "{}");

        var handler = new RecordAuditCommandHandler(_repo.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.AuditRecordId);
        _repo.Verify(r => r.AppendAsync(It.IsAny<AuditRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Record_WhenWhoActedIsEmpty_ReturnsFailure()
    {
        var cmd = new RecordAuditCommand(
            WhoActed: Guid.Empty,
            SubjectType: "User",
            WhatChanged: "Updated user name",
            EventType: "UserUpdated",
            AuditResult: "Success",
            AffectedEntityId: Guid.NewGuid(),
            AffectedEntityType: "UserAccount",
            RootTenantId: Guid.NewGuid(),
            Metadata: "{}");

        var handler = new RecordAuditCommandHandler(_repo.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Record_WhenWhatChangedIsEmpty_ReturnsFailure()
    {
        var cmd = new RecordAuditCommand(
            WhoActed: Guid.NewGuid(),
            SubjectType: "User",
            WhatChanged: "",
            EventType: "UserUpdated",
            AuditResult: "Success",
            AffectedEntityId: Guid.NewGuid(),
            AffectedEntityType: "UserAccount",
            RootTenantId: Guid.NewGuid(),
            Metadata: "{}");

        var handler = new RecordAuditCommandHandler(_repo.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}

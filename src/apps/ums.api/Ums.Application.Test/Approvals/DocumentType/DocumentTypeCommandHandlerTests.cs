namespace Ums.Application.Test.Approvals.DocumentType;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.DocumentType.Commands;
using Ums.Domain.Approvals.DocumentType;
using Ums.Domain.Kernel;
using Ums.Domain.Enums;
using Ums.Domain.Approvals;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DocumentTypeCommandHandlerTests
{
    private readonly Mock<IDocumentTypeRepository> _repo = new();
    private readonly Mock<IUnitOfWork>             _uow  = new();
    private readonly Mock<IUserContext>            _ctx  = new();

    public DocumentTypeCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    // =========================================================================
    #region CreateDocumentTypeCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new CreateDocumentTypeCommand(
            TenantId: Guid.NewGuid(),
            Code: "DOCTYPE-001",
            Name: "Passport",
            Description: "Government-issued passport",
            Criticity: "High");

        var handler = new CreateDocumentTypeCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.DocumentTypeId);
        _repo.Verify(r => r.AddAsync(It.IsAny<DocumentType>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateDocumentTypeCommand(
            TenantId: Guid.NewGuid(),
            Code: "DOCTYPE-001",
            Name: "Passport",
            Description: "Government-issued passport",
            Criticity: "High");

        var handler = new CreateDocumentTypeCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}

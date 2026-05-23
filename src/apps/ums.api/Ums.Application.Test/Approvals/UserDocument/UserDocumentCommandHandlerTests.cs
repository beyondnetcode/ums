namespace Ums.Application.Test.Approvals.UserDocument;

using Ums.Application.Common.Interfaces;
using Ums.Application.Approvals.UserDocument.Commands;
using Ums.Application.Approvals.UserDocument.DTOs;
using Ums.Domain.Approvals.UserDocument;
using Ums.Domain.Approvals;
using Ums.Domain.Enums;
using Ums.Domain.Kernel;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class UserDocumentCommandHandlerTests
{
    private readonly Mock<IUserDocumentRepository> _repo = new();
    private readonly Mock<IUnitOfWork>            _uow  = new();
    private readonly Mock<IUserContext>           _ctx  = new();

    public UserDocumentCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static UserDocument MakeUserDocument()
    {
        return UserDocument.Upload(
            UserId.Load(Guid.NewGuid()),
            DocumentTypeId.Load(Guid.NewGuid()),
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(10),
            DocumentCriticity.High,
            TextValueObject.Create("docs/my-file.pdf"),
            "MD5-12345",
            ActorId.Create("user-001")).Value;
    }

    // =========================================================================
    #region UploadUserDocumentCommandHandler
    // =========================================================================

    [Fact]
    public async Task Upload_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new UploadUserDocumentCommand(
            UserId: Guid.NewGuid(),
            DocumentTypeId: Guid.NewGuid(),
            IssueDate: DateTime.UtcNow.AddDays(-1),
            ExpirationDate: DateTime.UtcNow.AddDays(10),
            Criticity: "High",
            FileStoragePath: "docs/my-file.pdf",
            FileChecksum: "MD5-12345");

        var handler = new UploadUserDocumentCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.UserDocumentId);
        _repo.Verify(r => r.AddAsync(It.IsAny<UserDocument>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Upload_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new UploadUserDocumentCommand(
            UserId: Guid.NewGuid(),
            DocumentTypeId: Guid.NewGuid(),
            IssueDate: DateTime.UtcNow.AddDays(-1),
            ExpirationDate: DateTime.UtcNow.AddDays(10),
            Criticity: "High",
            FileStoragePath: "docs/my-file.pdf",
            FileChecksum: "MD5-12345");

        var handler = new UploadUserDocumentCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_WhenExpirationBeforeIssueDate_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");

        var cmd = new UploadUserDocumentCommand(
            UserId: Guid.NewGuid(),
            DocumentTypeId: Guid.NewGuid(),
            IssueDate: DateTime.UtcNow.AddDays(10),
            ExpirationDate: DateTime.UtcNow.AddDays(1),
            Criticity: "High",
            FileStoragePath: "docs/my-file.pdf",
            FileChecksum: "MD5-12345");

        var handler = new UploadUserDocumentCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region ValidateUserDocumentCommandHandler
    // =========================================================================

    [Fact]
    public async Task Validate_WithValidCommand_ReturnsSuccess()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var doc = MakeUserDocument();
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(doc);

        var cmd = new ValidateUserDocumentCommand(doc.Props.Id.GetValue());
        var handler = new ValidateUserDocumentCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatus.Valid, doc.Status);
        _repo.Verify(r => r.UpdateAsync(doc, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Validate_WhenNotFound_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((UserDocument?)null);

        var cmd = new ValidateUserDocumentCommand(Guid.NewGuid());
        var handler = new ValidateUserDocumentCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("user document not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Validate_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new ValidateUserDocumentCommand(Guid.NewGuid());
        var handler = new ValidateUserDocumentCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Validate_WhenNotPendingReview_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("user-001");
        var doc = MakeUserDocument();
        // Set it to valid first
        doc.Validate(ActorId.Create("user-001"));

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(doc);

        var cmd = new ValidateUserDocumentCommand(doc.Props.Id.GetValue());
        var handler = new ValidateUserDocumentCommandHandler(_repo.Object, _ctx.Object);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion
}

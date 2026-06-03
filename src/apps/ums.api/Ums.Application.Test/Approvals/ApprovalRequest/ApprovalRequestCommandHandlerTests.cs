namespace Ums.Application.Test.Approvals.ApprovalRequest;

using Ums.Application.Common.Interfaces;
using Ums.Application.Common.Notifications;
using Ums.Application.Approvals.ApprovalRequest.Commands;
using Ums.Application.Approvals.ApprovalRequest.DTOs;
using Ums.Application.Approvals.ApprovalRequest.Services;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Domain.Approvals;
using Ums.Domain.Enums;
using Ums.Domain.Identity;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ApprovalRequestCommandHandlerTests
{
    private readonly Mock<IApprovalRequestRepository>             _repo                   = new();
    private readonly Mock<IApprovalWorkflowRepository>            _workflowRepo           = new();
    private readonly Mock<IApprovalRequestCreationPolicyResolver> _creationPolicyResolver = new();
    private readonly Mock<IUserAccountRepository>                 _userAccountRepo        = new();
    private readonly Mock<ITenantRepository>                      _tenantRepo             = new();
    private readonly Mock<INotificationService>                   _notifications          = new();
    private readonly Mock<IUnitOfWork>                            _uow                    = new();
    private readonly Mock<IUserContext>                           _ctx                    = new();

    private static readonly Guid          TenantId     = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private static readonly SystemSuiteId ValidSystemId = SystemSuiteId.Load(Guid.NewGuid());
    private static readonly RoleId        ValidRoleId   = RoleId.Load(Guid.NewGuid());

    public ApprovalRequestCommandHandlerTests()
    {
        _repo.Setup(r => r.UnitOfWork).Returns(_uow.Object);
        _uow.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _ctx.Setup(u => u.UserId).Returns("user-001");
    }

    private static ApprovalRequest MakeApprovalRequest() =>
        ApprovalRequest.Create(
            ApprovalWorkflowId.Load(Guid.NewGuid()),
            UserId.Load(Guid.NewGuid()),
            ProfileId.Load(Guid.NewGuid()),
            ValidSystemId, null, ValidRoleId, null,
            ActorId.Create("user-001")).Value;

    private static ApprovalWorkflowAggregate MakeWorkflow(bool requiresApproval = true, UserCategory? category = null) =>
        ApprovalWorkflowAggregate.Create(
            Domain.Kernel.ValueObjects.TenantId.Load(Guid.NewGuid()),
            Code.Create($"wf-{Guid.NewGuid():N}"),
            Name.Create("Workflow"),
            Description.Create("Workflow description"),
            category ?? UserCategory.Internal,
            requiresApproval,
            null,
            ActorId.Create("user-001"),
            requiredDocumentCount: requiresApproval ? 1 : 0).Value;

    private static UserAccount MakeExternalUser(UserCategory? category = null)
    {
        var user = UserAccount.Create(
            Domain.Kernel.ValueObjects.TenantId.Load(TenantId),
            Email.Create("external@partner.com"),
            category ?? UserCategory.External,
            null, null,
            ActorId.Create("sys")).Value;
        user.Activate(ActorId.Create("sys"));
        return user;
    }

    private static UserAccount MakeInternalUser()
    {
        var user = UserAccount.Create(
            Domain.Kernel.ValueObjects.TenantId.Load(TenantId),
            Email.Create("internal@company.com"),
            UserCategory.Internal,
            null, null,
            ActorId.Create("sys")).Value;
        user.Activate(ActorId.Create("sys"));
        return user;
    }

    private Domain.Identity.Tenant.Tenant MakeTenant() =>
        Domain.Identity.Tenant.Tenant.Create(
            Code.Create("CORP"), Name.Create("Corp Inc"),
            OrganizationType.INTERNAL, ActorId.Create("sys"),
            tenantId: Domain.Kernel.ValueObjects.TenantId.Load(TenantId)).Value;

    private CreateApprovalRequestCommandHandler CreateHandler() =>
        new(_repo.Object, _workflowRepo.Object, _creationPolicyResolver.Object, _userAccountRepo.Object, _ctx.Object);

    private ApproveRequestCommandHandler CreateApproveHandler() =>
        new(_repo.Object, _userAccountRepo.Object, _tenantRepo.Object, _notifications.Object, _ctx.Object);

    private RejectRequestCommandHandler CreateRejectHandler() =>
        new(_repo.Object, _userAccountRepo.Object, _tenantRepo.Object, _notifications.Object, _ctx.Object);

    // =========================================================================
    #region CreateApprovalRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_WithValidCommand_ReturnsSuccess()
    {
        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(),
            TargetUserId: Guid.NewGuid(),
            TargetProfileId: Guid.NewGuid(),
            RequestedSystemId: Guid.NewGuid(),
            RequestedBranchId: null,
            RequestedRoleId: Guid.NewGuid(),
            Justification: null);

        var workflow       = MakeWorkflow();
        var internalUser   = MakeInternalUser();
        var createdRequest = MakeApprovalRequest();

        _workflowRepo.Setup(r => r.GetByIdAsync(cmd.WorkflowId, It.IsAny<CancellationToken>())).ReturnsAsync(workflow);
        _userAccountRepo.Setup(r => r.GetByIdAsync(cmd.TargetUserId, It.IsAny<CancellationToken>())).ReturnsAsync(internalUser);
        _repo.Setup(r => r.ExistsPendingForScopeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _creationPolicyResolver.Setup(r => r.Create(
                workflow,
                It.IsAny<UserId>(),
                It.IsAny<ProfileId?>(),
                It.IsAny<SystemSuiteId>(),
                It.IsAny<BranchId?>(),
                It.IsAny<RoleId>(),
                It.IsAny<string?>(),
                It.IsAny<ActorId>()))
            .Returns(Result<ApprovalRequest>.Success(createdRequest));

        var result = await CreateHandler().Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.ApprovalRequestId);
        _repo.Verify(r => r.AddAsync(It.IsAny<ApprovalRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(), TargetUserId: Guid.NewGuid(),
            TargetProfileId: Guid.NewGuid(), RequestedSystemId: Guid.NewGuid(),
            RequestedBranchId: null, RequestedRoleId: Guid.NewGuid(), Justification: null);

        var result = await CreateHandler().Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenWorkflowNotFound_ReturnsFailure()
    {
        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(), TargetUserId: Guid.NewGuid(),
            TargetProfileId: Guid.NewGuid(), RequestedSystemId: Guid.NewGuid(),
            RequestedBranchId: null, RequestedRoleId: Guid.NewGuid(), Justification: null);

        _workflowRepo.Setup(r => r.GetByIdAsync(cmd.WorkflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApprovalWorkflowAggregate?)null);

        var result = await CreateHandler().Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("approval workflow not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenTargetUserNotFound_ReturnsFailure()
    {
        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(), TargetUserId: Guid.NewGuid(),
            TargetProfileId: Guid.NewGuid(), RequestedSystemId: Guid.NewGuid(),
            RequestedBranchId: null, RequestedRoleId: Guid.NewGuid(), Justification: null);

        _workflowRepo.Setup(r => r.GetByIdAsync(cmd.WorkflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeWorkflow());
        _userAccountRepo.Setup(r => r.GetByIdAsync(cmd.TargetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount?)null);

        var result = await CreateHandler().Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WhenInternalWorkflowUsedByExternalUser_ReturnsPrivilegeEscalationError()
    {
        var externalUser   = MakeExternalUser();
        var internalWorkflow = MakeWorkflow(category: UserCategory.Internal);

        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(), TargetUserId: Guid.NewGuid(),
            TargetProfileId: Guid.NewGuid(), RequestedSystemId: Guid.NewGuid(),
            RequestedBranchId: null, RequestedRoleId: Guid.NewGuid(), Justification: "B2B access");

        _workflowRepo.Setup(r => r.GetByIdAsync(cmd.WorkflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(internalWorkflow);
        _userAccountRepo.Setup(r => r.GetByIdAsync(cmd.TargetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalUser);

        var result = await CreateHandler().Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.WorkflowNotAllowedForUserCategory, result.Error);
    }

    [Fact]
    public async Task Create_WhenInternalWorkflowUsedByB2BUser_ReturnsPrivilegeEscalationError()
    {
        var b2bUser          = MakeExternalUser(UserCategory.B2B);
        var internalWorkflow = MakeWorkflow(category: UserCategory.Internal);

        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(), TargetUserId: Guid.NewGuid(),
            TargetProfileId: null, RequestedSystemId: Guid.NewGuid(),
            RequestedBranchId: null, RequestedRoleId: Guid.NewGuid(), Justification: "B2B");

        _workflowRepo.Setup(r => r.GetByIdAsync(cmd.WorkflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(internalWorkflow);
        _userAccountRepo.Setup(r => r.GetByIdAsync(cmd.TargetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(b2bUser);

        var result = await CreateHandler().Handle(cmd, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(DomainErrors.Approvals.WorkflowNotAllowedForUserCategory, result.Error);
    }

    [Fact]
    public async Task Create_WhenExternalWorkflowUsedByExternalUser_Succeeds()
    {
        var externalUser     = MakeExternalUser();
        var externalWorkflow = MakeWorkflow(category: UserCategory.External);
        var createdRequest   = MakeApprovalRequest();

        var cmd = new CreateApprovalRequestCommand(
            WorkflowId: Guid.NewGuid(), TargetUserId: Guid.NewGuid(),
            TargetProfileId: null, RequestedSystemId: Guid.NewGuid(),
            RequestedBranchId: null, RequestedRoleId: Guid.NewGuid(), Justification: "B2B");

        _workflowRepo.Setup(r => r.GetByIdAsync(cmd.WorkflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalWorkflow);
        _userAccountRepo.Setup(r => r.GetByIdAsync(cmd.TargetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalUser);
        _repo.Setup(r => r.ExistsPendingForScopeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _creationPolicyResolver.Setup(r => r.Create(
                externalWorkflow,
                It.IsAny<UserId>(), It.IsAny<ProfileId?>(), It.IsAny<SystemSuiteId>(),
                It.IsAny<BranchId?>(), It.IsAny<RoleId>(), It.IsAny<string?>(), It.IsAny<ActorId>()))
            .Returns(Result<ApprovalRequest>.Success(createdRequest));

        var result = await CreateHandler().Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion

    // =========================================================================
    #region ApproveRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Approve_WithValidCommand_ReturnsSuccess()
    {
        var req  = MakeApprovalRequest();
        var user = MakeExternalUser();
        var tenant = MakeTenant();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(req);
        _userAccountRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tenantRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await CreateApproveHandler().Handle(
            new ApproveRequestCommand(req.Props.Id.GetValue(), ValidRoleId.GetValue()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Approved, req.Status);
        _repo.Verify(r => r.UpdateAsync(req, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_WithValidCommand_SendsApprovalNotificationToApplicant()
    {
        var req    = MakeApprovalRequest();
        var user   = MakeExternalUser();
        var tenant = MakeTenant();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(req);
        _userAccountRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tenantRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        await CreateApproveHandler().Handle(
            new ApproveRequestCommand(req.Props.Id.GetValue(), ValidRoleId.GetValue()),
            CancellationToken.None);

        _notifications.Verify(
            n => n.SendAsync(
                It.Is<UmsNotification>(n => n.Recipient == "external@partner.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Approve_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ApprovalRequest?)null);

        var result = await CreateApproveHandler().Handle(
            new ApproveRequestCommand(Guid.NewGuid(), ValidRoleId.GetValue()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("approval request not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Approve_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var result = await CreateApproveHandler().Handle(
            new ApproveRequestCommand(Guid.NewGuid(), ValidRoleId.GetValue()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Approve_WhenNotPending_ReturnsFailure()
    {
        var req = MakeApprovalRequest();
        req.Approve(ActorId.Create("user-001"), ValidRoleId);

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(req);

        var result = await CreateApproveHandler().Handle(
            new ApproveRequestCommand(req.Props.Id.GetValue(), ValidRoleId.GetValue()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    #endregion

    // =========================================================================
    #region RejectRequestCommandHandler
    // =========================================================================

    [Fact]
    public async Task Reject_WithValidCommand_ReturnsSuccess()
    {
        var req    = MakeApprovalRequest();
        var user   = MakeExternalUser();
        var tenant = MakeTenant();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(req);
        _userAccountRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tenantRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await CreateRejectHandler().Handle(
            new RejectRequestCommand(req.Props.Id.GetValue()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApprovalStatus.Rejected, req.Status);
        _repo.Verify(r => r.UpdateAsync(req, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_WithValidCommand_SendsDenialNotificationToApplicant()
    {
        var req    = MakeApprovalRequest();
        var user   = MakeExternalUser();
        var tenant = MakeTenant();

        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(req);
        _userAccountRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tenantRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        await CreateRejectHandler().Handle(
            new RejectRequestCommand(req.Props.Id.GetValue(), "Not eligible"),
            CancellationToken.None);

        _notifications.Verify(
            n => n.SendAsync(
                It.Is<UmsNotification>(n => n.Recipient == "external@partner.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Reject_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ApprovalRequest?)null);

        var result = await CreateRejectHandler().Handle(
            new RejectRequestCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("approval request not found", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Reject_WhenUnauthenticated_ReturnsFailure()
    {
        _ctx.Setup(u => u.UserId).Returns("");

        var result = await CreateRejectHandler().Handle(
            new RejectRequestCommand(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("authenticated user is required", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}

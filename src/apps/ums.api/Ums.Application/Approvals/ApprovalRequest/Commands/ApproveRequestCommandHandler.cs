using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Commands;

using Ums.Application.Common.Notifications;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Approvals;
using Ums.Domain.Identity;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Identity.UserManagementDelegation;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Events;

public sealed class ApproveRequestCommandHandler : ICommandHandler<ApproveRequestCommand>
{
    private readonly IApprovalRequestRepository _repository;
    private readonly IProfileRepository _profileRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserManagementDelegationRepository _delegationRepository;
    private readonly ITenantScopePolicy _tenantScopePolicy;
    private readonly IUnitOfWorkScope _unitOfWorkScope;
    private readonly INotificationService _notificationService;
    private readonly IUserContext _userContext;

    public ApproveRequestCommandHandler(
        IApprovalRequestRepository repository,
        IProfileRepository profileRepository,
        IUserAccountRepository userAccountRepository,
        ITenantRepository tenantRepository,
        IUserManagementDelegationRepository delegationRepository,
        ITenantScopePolicy tenantScopePolicy,
        IUnitOfWorkScope unitOfWorkScope,
        INotificationService notificationService,
        IUserContext userContext)
    {
        _repository = repository;
        _profileRepository = profileRepository;
        _userAccountRepository = userAccountRepository;
        _tenantRepository = tenantRepository;
        _delegationRepository = delegationRepository;
        _tenantScopePolicy = tenantScopePolicy;
        _unitOfWorkScope = unitOfWorkScope;
        _notificationService = notificationService;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(ApproveRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
            return Result.Failure("Authenticated user is required.");

        var entity = await _repository.GetByIdAsync(request.ApprovalRequestId, cancellationToken);
        if (entity is null) return Result.Failure("Approval request not found.");

        var targetUser = await _userAccountRepository.GetByIdAsync(entity.TargetUserId.GetValue(), cancellationToken);
        if (targetUser is null)
            return Result.Failure("Target user not found.");

        var authorization = await _tenantScopePolicy.EnsureManagementOwnerScopeAsync(targetUser.TenantId.GetValue(), cancellationToken);
        if (authorization.IsFailure && !await CanApproveAsDelegatedBranchManagerAsync(entity, targetUser.TenantId.GetValue(), cancellationToken))
            return Result.Failure(authorization.Error);

        var result = entity.Approve(ActorId.Create(_userContext.UserId), RoleId.Load(request.GrantedRoleId), request.DecisionReason);
        if (result.IsFailure) return result;

        var grantedRoleId = RoleId.Load(request.GrantedRoleId);
        var assignedProfileResult = await ResolveOrCreateProfileAsync(entity, targetUser, grantedRoleId, cancellationToken);
        if (assignedProfileResult.IsFailure)
            return Result.Failure(assignedProfileResult.Error);

        var (assignedProfile, isNewProfile) = assignedProfileResult.Value;
        entity.Props.TargetProfileId = assignedProfile.GetId();
        entity.DomainEvents.RaiseEvent(new ProfileAssignedToUserEvent(
            targetUser.TenantId.GetValue(),
            entity.Props.Id.GetValue(),
            targetUser.GetId().GetValue(),
            assignedProfile.GetId().GetValue(),
            grantedRoleId.GetValue(),
            entity.RequestedBranchId?.GetValue(),
            _userContext.UserId,
            DateTime.UtcNow));

        await using var tx = await _unitOfWorkScope.BeginAsync(cancellationToken);
        if (isNewProfile)
            await _profileRepository.AddAsync(assignedProfile, cancellationToken);
        else
            await _profileRepository.UpdateAsync(assignedProfile, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var tenant = await _tenantRepository.GetByIdAsync(targetUser.TenantId.GetValue(), cancellationToken);
        await _notificationService.SendAsync(
            NotificationTemplates.ProfileRequestApproved(
                targetUser.Email.GetValue(),
                targetUser.DisplayName?.GetValue() ?? targetUser.Email.GetValue(),
                tenant?.Name.GetValue() ?? "your organization",
                entity.RequestedSystemId.GetValue().ToString(),
                entity.GrantedRoleId!.GetValue().ToString()),
            cancellationToken);

        return Result.Success();
    }

    private async Task<bool> CanApproveAsDelegatedBranchManagerAsync(
        ApprovalRequest entity,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_userContext.UserId, out var delegatedAdminId))
        {
            return false;
        }

        var delegations = await _delegationRepository.GetActiveAsync(tenantId, cancellationToken);
        return delegations.Any(delegation =>
            delegation.Props.DelegatedAdminId.GetValue() == delegatedAdminId &&
            delegation.CanExecuteAction(DelegatedAction.AssignProfile, entity.RequestedBranchId?.GetValue()));
    }

    private async Task<Result<(Profile Profile, bool IsNew)>> ResolveOrCreateProfileAsync(
        ApprovalRequest entity,
        UserAccount targetUser,
        RoleId grantedRoleId,
        CancellationToken cancellationToken)
    {
        var requestedBranchId = entity.RequestedBranchId?.GetValue();
        var existingProfiles = await _profileRepository.GetByUserIdAsync(targetUser.GetId().GetValue(), cancellationToken);
        var existingProfile = existingProfiles.FirstOrDefault(profile =>
            profile.IsActive &&
            profile.RoleId.GetValue() == grantedRoleId.GetValue() &&
            profile.BranchId?.GetValue() == requestedBranchId);

        if (existingProfile is not null)
        {
            return Result<(Profile Profile, bool IsNew)>.Success((existingProfile, false));
        }

        var profileResult = Profile.Create(
            targetUser.TenantId,
            UserId.Load(targetUser.GetId().GetValue()),
            grantedRoleId,
            entity.RequestedBranchId is null ? null : BranchId.Load(entity.RequestedBranchId.GetValue()),
            ActorId.Create(_userContext.UserId));

        if (profileResult.IsFailure)
        {
            return Result<(Profile Profile, bool IsNew)>.Failure(profileResult.Error);
        }

        return Result<(Profile Profile, bool IsNew)>.Success((profileResult.Value, true));
    }
}

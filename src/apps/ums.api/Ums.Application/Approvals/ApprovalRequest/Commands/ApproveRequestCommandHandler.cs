using Ums.Application.Approvals.ApprovalRequest.DTOs;

namespace Ums.Application.Approvals.ApprovalRequest.Commands;

using Ums.Application.Common.Notifications;
using Ums.Domain.Approvals;
using Ums.Domain.Identity;
using Ums.Domain.Kernel.ValueObjects;

public sealed class ApproveRequestCommandHandler : ICommandHandler<ApproveRequestCommand>
{
    private readonly IApprovalRequestRepository _repository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly INotificationService _notificationService;
    private readonly IUserContext _userContext;

    public ApproveRequestCommandHandler(
        IApprovalRequestRepository repository,
        IUserAccountRepository userAccountRepository,
        ITenantRepository tenantRepository,
        INotificationService notificationService,
        IUserContext userContext)
    {
        _repository = repository;
        _userAccountRepository = userAccountRepository;
        _tenantRepository = tenantRepository;
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

        var result = entity.Approve(ActorId.Create(_userContext.UserId), RoleId.Load(request.GrantedRoleId), request.DecisionReason);
        if (result.IsFailure) return result;

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        var targetUser = await _userAccountRepository.GetByIdAsync(entity.TargetUserId.GetValue(), cancellationToken);
        if (targetUser is not null)
        {
            var tenant = await _tenantRepository.GetByIdAsync(targetUser.TenantId.GetValue(), cancellationToken);
            await _notificationService.SendAsync(
                NotificationTemplates.ProfileRequestApproved(
                    targetUser.Email.GetValue(),
                    targetUser.DisplayName?.GetValue() ?? targetUser.Email.GetValue(),
                    tenant?.Name.GetValue() ?? "your organization",
                    entity.RequestedSystemId.GetValue().ToString(),
                    entity.GrantedRoleId!.GetValue().ToString()),
                cancellationToken);
        }

        return Result.Success();
    }
}

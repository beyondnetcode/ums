namespace Ums.Application.Authorization.Profile.Commands;

using Ums.Application.Common.Interfaces;
using Ums.Domain.Authorization;

public sealed class AssignProfileTemplateCommandHandler : ICommandHandler<AssignProfileTemplateCommand>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IPermissionTemplateRepository _permissionTemplateRepository;
    private readonly IUserContext _userContext;
    private readonly ITenantScopePolicy _tenantScopePolicy;
    private readonly IUserManagementDelegationAccessService _delegationAccessService;

    public AssignProfileTemplateCommandHandler(
        IProfileRepository profileRepository,
        IPermissionTemplateRepository permissionTemplateRepository,
        IUserContext userContext,
        ITenantScopePolicy tenantScopePolicy,
        IUserManagementDelegationAccessService delegationAccessService)
    {
        _profileRepository = profileRepository;
        _permissionTemplateRepository = permissionTemplateRepository;
        _userContext = userContext;
        _tenantScopePolicy = tenantScopePolicy;
        _delegationAccessService = delegationAccessService;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(AssignProfileTemplateCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to assign a template.");
        }

        var profile = await _profileRepository.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure("Profile was not found.");
        }

        var ownerScopeResult = await _tenantScopePolicy.EnsureManagementOwnerScopeAsync(profile.TenantId.GetValue(), cancellationToken);
        if (ownerScopeResult.IsFailure)
        {
            var delegatedAccess = await _delegationAccessService.EnsureCanExecuteAsync(
                profile.TenantId.GetValue(),
                Ums.Domain.Identity.UserManagementDelegation.DelegatedAction.AssignProfile,
                profile.BranchId?.GetValue(),
                cancellationToken);

            if (delegatedAccess.IsFailure)
            {
                return Result.Failure(ownerScopeResult.Error);
            }
        }

        var template = await _permissionTemplateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template is null)
        {
            return Result.Failure("Permission template was not found.");
        }

        var result = profile.AssignTemplate(template, ActorId.Create(_userContext.UserId));
        if (result.IsFailure)
        {
            return result;
        }

        await _profileRepository.UpdateAsync(profile, cancellationToken);
        await _profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}

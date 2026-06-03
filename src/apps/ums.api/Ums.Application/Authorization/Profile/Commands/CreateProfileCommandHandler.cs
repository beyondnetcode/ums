using Ums.Application.Authorization.Profile.DTOs;

namespace Ums.Application.Authorization.Profile.Commands;

using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Events;

public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, CreateProfileResponse>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IPermissionTemplateRepository _templateRepository;
    private readonly ITemplateAssignmentRuleRepository _assignmentRuleRepository;
    private readonly IUserContext _userContext;
    private readonly ITenantScopePolicy _tenantScopePolicy;

    public CreateProfileCommandHandler(
        IProfileRepository profileRepository,
        IPermissionTemplateRepository templateRepository,
        ITemplateAssignmentRuleRepository assignmentRuleRepository,
        IUserContext userContext,
        ITenantScopePolicy tenantScopePolicy)
    {
        _profileRepository = profileRepository;
        _templateRepository = templateRepository;
        _assignmentRuleRepository = assignmentRuleRepository;
        _userContext = userContext;
        _tenantScopePolicy = tenantScopePolicy;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<CreateProfileResponse>> Handle(
        CreateProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result<CreateProfileResponse>.Failure("Authenticated user is required to create a profile.");
        }

        var scopeResult = await _tenantScopePolicy.EnsureManagementOwnerScopeAsync(request.TenantId, cancellationToken);
        if (scopeResult.IsFailure)
        {
            return Result<CreateProfileResponse>.Failure(scopeResult.Error);
        }

        var profileResult = Profile.Create(
            TenantId.Load(request.TenantId),
            UserId.Load(request.UserId),
            RoleId.Load(request.RoleId),
            request.BranchId.HasValue ? BranchId.Load(request.BranchId.Value) : null,
            ActorId.Create(_userContext.UserId));

        if (profileResult.IsFailure)
        {
            return Result<CreateProfileResponse>.Failure(profileResult.Error);
        }

        var profile = profileResult.Value;

        await _profileRepository.AddAsync(profile, cancellationToken);
        await _profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        await TryAutoAssignTemplateAsync(profile, request.TenantId, request.RoleId, cancellationToken);

        return Result<CreateProfileResponse>.Success(
            new CreateProfileResponse(profile.Props.Id.GetValue()));
    }

    private async Task TryAutoAssignTemplateAsync(
        Profile profile,
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var matchingRules = await _assignmentRuleRepository.GetActiveByTenantAndRoleAsync(
            tenantId, roleId, cancellationToken);

        var topRule = matchingRules?.FirstOrDefault();
        if (topRule is null)
        {
            return;
        }

        var template = await _templateRepository.GetByIdAsync(topRule.TemplateId.GetValue(), cancellationToken);
        if (template is null)
        {
            return;
        }

        var assignResult = profile.AssignTemplate(template, ActorId.Create(_userContext.UserId));
        if (assignResult.IsFailure)
        {
            return;
        }

        profile.DomainEvents.RaiseEvent(new TemplateAutoAssignedEvent(
            profile.Props.Id.GetValue(),
            template.Props.Id.GetValue(),
            topRule.Props.Id.GetValue()));

        await _profileRepository.UpdateAsync(profile, cancellationToken);
        await _profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}

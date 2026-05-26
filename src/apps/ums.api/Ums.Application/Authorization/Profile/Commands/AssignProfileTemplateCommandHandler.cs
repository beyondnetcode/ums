namespace Ums.Application.Authorization.Profile.Commands;

using Ums.Domain.Authorization;

public sealed class AssignProfileTemplateCommandHandler : ICommandHandler<AssignProfileTemplateCommand>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IPermissionTemplateRepository _permissionTemplateRepository;
    private readonly IUserContext _userContext;

    public AssignProfileTemplateCommandHandler(
        IProfileRepository profileRepository,
        IPermissionTemplateRepository permissionTemplateRepository,
        IUserContext userContext)
    {
        _profileRepository = profileRepository;
        _permissionTemplateRepository = permissionTemplateRepository;
        _userContext = userContext;
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

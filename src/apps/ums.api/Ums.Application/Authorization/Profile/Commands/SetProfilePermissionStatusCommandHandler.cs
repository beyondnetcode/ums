namespace Ums.Application.Authorization.Profile.Commands;

using Ums.Domain.Authorization;

public sealed class SetProfilePermissionStatusCommandHandler : ICommandHandler<SetProfilePermissionStatusCommand>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUserContext _userContext;

    public SetProfilePermissionStatusCommandHandler(IProfileRepository profileRepository, IUserContext userContext)
    {
        _profileRepository = profileRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(SetProfilePermissionStatusCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to change permission status.");
        }

        var profile = await _profileRepository.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure("Profile was not found.");
        }

        var actor = ActorId.Create(_userContext.UserId);
        var permissionId = IdValueObject.Load(request.PermissionId);
        var result = request.IsActive
            ? profile.ActivatePermission(permissionId, actor)
            : profile.DeactivatePermission(permissionId, actor);

        if (result.IsFailure)
        {
            return result;
        }

        await _profileRepository.UpdateAsync(profile, cancellationToken);
        await _profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}

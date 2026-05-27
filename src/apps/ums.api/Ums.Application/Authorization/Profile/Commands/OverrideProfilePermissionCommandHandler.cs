namespace Ums.Application.Authorization.Profile.Commands;

using Ums.Domain.Authorization;

public sealed class OverrideProfilePermissionCommandHandler : ICommandHandler<OverrideProfilePermissionCommand>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUserContext _userContext;

    public OverrideProfilePermissionCommandHandler(IProfileRepository profileRepository, IUserContext userContext)
    {
        _profileRepository = profileRepository;
        _userContext = userContext;
    }

    [AuditTrail]
    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result> Handle(OverrideProfilePermissionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_userContext.UserId))
        {
            return Result.Failure("Authenticated user is required to override a permission.");
        }

        var profile = await _profileRepository.GetByIdAsync(request.ProfileId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure("Profile was not found.");
        }

        var actor = ActorId.Create(_userContext.UserId);
        var permissionId = IdValueObject.Load(request.PermissionId);
        Result result = request.Effect.Trim().ToLowerInvariant() switch
        {
            "allow" => profile.OverridePermissionAllow(permissionId, actor),
            "deny" => profile.OverridePermissionDeny(permissionId, actor),
            "neutral" => profile.OverridePermissionNeutral(permissionId, actor),
            _ => Result.Failure("Permission effect is invalid.")
        };

        if (result.IsFailure)
        {
            return result;
        }

        await _profileRepository.UpdateAsync(profile, cancellationToken);
        await _profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Result.Success();
    }
}

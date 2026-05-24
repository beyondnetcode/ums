using Ums.Application.Authorization.Profile.DTOs;

namespace Ums.Application.Authorization.Profile.Commands;

using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;

public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, CreateProfileResponse>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUserContext _userContext;

    public CreateProfileCommandHandler(
        IProfileRepository profileRepository,
        IUserContext userContext)
    {
        _profileRepository = profileRepository;
        _userContext = userContext;
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

        await _profileRepository.AddAsync(profileResult.Value, cancellationToken);
        await _profileRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<CreateProfileResponse>.Success(
            new CreateProfileResponse(profileResult.Value.Props.Id.GetValue()));
    }
}

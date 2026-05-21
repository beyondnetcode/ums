using Ums.Application.Authorization.Profile.DTOs;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;

namespace Ums.Application.Authorization.Profile.Queries;

public sealed class GetProfileByIdQueryHandler : IQueryHandler<GetProfileByIdQuery, ProfileDto>
{
    private readonly IProfileRepository _profileRepository;

    public GetProfileByIdQueryHandler(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<Result<ProfileDto>> Handle(
        GetProfileByIdQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(request.ProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<ProfileDto>.Failure("Profile not found.");
        }

        return Result<ProfileDto>.Success(new ProfileDto(
            profile.Props.Id.GetValue(),
            profile.Props.TenantId.GetValue(),
            profile.Props.UserId.GetValue(),
            profile.Props.RoleId.GetValue(),
            profile.Props.BranchId?.GetValue(),
            profile.Props.Scope.ToString(),
            profile.Props.IsActive));
    }
}

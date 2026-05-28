using Ums.Application.Authorization.Profile.DTOs;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;

namespace Ums.Application.Authorization.Profile.Queries;

public sealed class GetProfileByIdQueryHandler : IQueryHandler<GetProfileByIdQuery, ProfileDto>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ISystemSuiteRepository _systemSuiteRepository;

    public GetProfileByIdQueryHandler(
        IProfileRepository profileRepository,
        IRoleRepository roleRepository,
        ISystemSuiteRepository systemSuiteRepository)
    {
        _profileRepository = profileRepository;
        _roleRepository = roleRepository;
        _systemSuiteRepository = systemSuiteRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
    public async Task<Result<ProfileDto>> Handle(
        GetProfileByIdQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByIdAsync(request.ProfileId, cancellationToken);

        if (profile is null)
        {
            return Result<ProfileDto>.Failure("Profile not found.");
        }

        // Resolve SystemSuite through Role to fetch target and action names
        var role = await _roleRepository.GetByIdAsync(profile.Props.RoleId.GetValue(), cancellationToken);
        var suite = role is null ? null : await _systemSuiteRepository.GetByIdAsync(role.Props.SystemSuiteId.GetValue(), cancellationToken);

        // Build flat action lookup: actionId → actionName
        var actionLookup = suite is null
            ? new Dictionary<Guid, string>()
            : suite.Actions.ToDictionary(
                a => a.GetId().GetValue(),
                a => a.Name.GetValue());

        // Build flat target lookup: id → name
        var targetLookup = BuildTargetNameLookup(suite);

        var permissions = profile.Permissions
            .Select(p => new ProfilePermissionDto(
                p.Props.Id.GetValue(),
                p.Props.ProfileId.GetValue(),
                p.Props.TemplateId.GetValue(),
                p.TargetType.Name,
                p.TargetId.GetValue(),
                targetLookup.GetValueOrDefault(p.TargetId.GetValue(), "—"),
                p.ActionId.GetValue(),
                actionLookup.GetValueOrDefault(p.ActionId.GetValue(), "—"),
                p.IsAllowed,
                p.IsDenied,
                p.IsActive,
                p.IsOverride))
            .ToList();

        return Result<ProfileDto>.Success(new ProfileDto(
            profile.Props.Id.GetValue(),
            profile.Props.TenantId.GetValue(),
            profile.Props.UserId.GetValue(),
            profile.Props.RoleId.GetValue(),
            profile.Props.BranchId?.GetValue(),
            profile.Props.Scope.ToString(),
            profile.Props.IsActive,
            permissions));
    }

    private static Dictionary<Guid, string> BuildTargetNameLookup(SystemSuiteAggregate? suite)
    {
        var lookup = new Dictionary<Guid, string>();
        if (suite is null) return lookup;

        foreach (var module in suite.Modules)
        {
            lookup[module.Props.Id.GetValue()] = module.Name.GetValue();

            foreach (var menu in module.Menus)
            {
                lookup[menu.Props.Id.GetValue()] = menu.Label.GetValue();

                foreach (var subMenu in menu.SubMenus)
                {
                    lookup[subMenu.Props.Id.GetValue()] = subMenu.Label.GetValue();

                    foreach (var option in subMenu.Options)
                    {
                        lookup[option.Props.Id.GetValue()] = option.Label.GetValue();
                    }
                }
            }
        }

        return lookup;
    }
}

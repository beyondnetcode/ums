using Ums.Application.Authorization.Template.DTOs;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Template;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;

namespace Ums.Application.Authorization.Template.Queries;

public sealed class GetPermissionTemplateByIdQueryHandler : IQueryHandler<GetPermissionTemplateByIdQuery, PermissionTemplateDetailDto>
{
    private readonly IPermissionTemplateRepository _templateRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ISystemSuiteRepository _systemSuiteRepository;

    public GetPermissionTemplateByIdQueryHandler(
        IPermissionTemplateRepository templateRepository,
        IRoleRepository roleRepository,
        ISystemSuiteRepository systemSuiteRepository)
    {
        _templateRepository = templateRepository;
        _roleRepository = roleRepository;
        _systemSuiteRepository = systemSuiteRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PermissionTemplateDetailDto>> Handle(
        GetPermissionTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);

        if (template is null)
        {
            return Result<PermissionTemplateDetailDto>.Failure("Permission template not found.");
        }

        // Resolve role and suite names
        var role = await _roleRepository.GetByIdAsync(template.Props.RoleId.GetValue(), cancellationToken);
        var roleName = role?.Props.Value.GetValue() ?? "—";

        var suite = await _systemSuiteRepository.GetByIdAsync(template.Props.SystemSuiteId.GetValue(), cancellationToken);
        var suiteName = suite?.Props.Name.GetValue() ?? "—";

        // Build flat action lookup: actionId → actionName
        var actionLookup = suite is null
            ? new Dictionary<Guid, string>()
            : suite.Actions.ToDictionary(
                a => a.GetId().GetValue(),
                a => a.Name.GetValue());

        // Build flat target lookup: id → name (across all nesting levels)
        var targetLookup = BuildTargetNameLookup(suite);

        var items = template.Items
            .Select(i => new PermissionTemplateItemDto(
                i.Props.Id.GetValue(),
                i.TargetType.Name,
                i.TargetId.GetValue(),
                targetLookup.GetValueOrDefault(i.TargetId.GetValue(), "—"),
                i.ActionId.GetValue(),
                actionLookup.GetValueOrDefault(i.ActionId.GetValue(), "—"),
                i.IsAllowed,
                i.IsDenied,
                i.IsActive))
            .ToList();

        return Result<PermissionTemplateDetailDto>.Success(new PermissionTemplateDetailDto(
            template.Props.Id.GetValue(),
            template.Props.TenantId.GetValue(),
            template.Props.RoleId.GetValue(),
            roleName,
            template.Props.SystemSuiteId.GetValue(),
            suiteName,
            template.Props.Version.GetValue(),
            template.Props.Status.ToString(),
            items));
    }

    /// <summary>
    /// Walks the SystemSuite tree and builds a flat id→displayName lookup
    /// covering Modules, Menus, SubMenus, and Options.
    /// </summary>
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

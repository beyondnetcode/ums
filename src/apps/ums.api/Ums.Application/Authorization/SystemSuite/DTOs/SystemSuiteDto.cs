using System;
using System.Collections.Generic;
using System.Linq;

namespace Ums.Application.Authorization.SystemSuite.DTOs;

public sealed record SystemSuiteDto(
    Guid SystemSuiteId,
    Guid TenantId,
    string Code,
    string Name,
    string Description,
    string Status,
    IReadOnlyList<SystemSuiteModuleDto> Modules,
    IReadOnlyList<SystemSuiteActionDto> Actions,
    IReadOnlyList<SystemSuiteDomainResourceDto> DomainResources)
{
    public static SystemSuiteDto Map(Ums.Domain.Authorization.SystemSuite.SystemSuite suite)
    {
        return new SystemSuiteDto(
            suite.Props.Id.GetValue(),
            suite.Props.TenantId.GetValue(),
            suite.Props.Code.GetValue(),
            suite.Props.Name.GetValue(),
            suite.Props.Description.GetValue(),
            suite.Props.Status.ToString(),
            suite.Modules.Select(m => new SystemSuiteModuleDto(
                m.Props.Id.GetValue(),          // Props.Id = stable DB GUID (Entity.Id is transient per-rehydration)
                m.Code.GetValue(),
                m.Name.GetValue(),
                m.Description.GetValue(),
                m.Status.ToString(),
                m.SortOrder,
                m.Menus.Select(menu => new SystemSuiteMenuDto(
                    menu.Props.Id.GetValue(),   // Props.Id = stable DB GUID
                    menu.Code.GetValue(),
                    menu.Label.GetValue(),
                    menu.Description.GetValue(),
                    menu.SortOrder,
                    menu.SubMenus.Select(sm => new SystemSuiteSubMenuDto(
                        sm.Props.Id.GetValue(), // Props.Id = stable DB GUID
                        sm.Code.GetValue(),
                        sm.Label.GetValue(),
                        sm.Description.GetValue(),
                        sm.SortOrder,
                        sm.Options.Select(opt => new SystemSuiteOptionDto(
                            opt.Props.Id.GetValue(), // Props.Id = stable DB GUID
                            opt.Code.GetValue(),
                            opt.Label.GetValue(),
                            opt.Description.GetValue(),
                            opt.ActionCode.GetValue(),
                            opt.SortOrder
                        )).ToList()
                    )).ToList()
                )).ToList()
            )).ToList(),
            suite.Actions.Select(a => new SystemSuiteActionDto(
                a.Props.Id.GetValue(),          // Props.Id = stable DB GUID
                a.Code.GetValue(),
                a.Name.GetValue()
            )).ToList(),
            suite.DomainResources.Select(dr => new SystemSuiteDomainResourceDto(
                dr.Props.Id.GetValue(),
                dr.ModuleId?.GetValue(),
                dr.ParentResourceId?.GetValue(),
                dr.Type.Name,
                dr.Code.GetValue(),
                dr.Name.GetValue(),
                dr.Description.GetValue()
            )).ToList()
        );
    }
}

public sealed record SystemSuiteModuleDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string Status,
    int SortOrder,
    IReadOnlyList<SystemSuiteMenuDto> Menus);

public sealed record SystemSuiteMenuDto(
    Guid Id,
    string Code,
    string Label,
    string Description,
    int SortOrder,
    IReadOnlyList<SystemSuiteSubMenuDto> SubMenus);

public sealed record SystemSuiteSubMenuDto(
    Guid Id,
    string Code,
    string Label,
    string Description,
    int SortOrder,
    IReadOnlyList<SystemSuiteOptionDto> Options);

public sealed record SystemSuiteOptionDto(
    Guid Id,
    string Code,
    string Label,
    string Description,
    string ActionCode,
    int SortOrder);

public sealed record SystemSuiteActionDto(
    Guid Id,
    string Code,
    string Name);

public sealed record SystemSuiteDomainResourceDto(
    Guid Id,
    Guid? ModuleId,
    Guid? ParentResourceId,
    string Type,
    string Code,
    string Name,
    string Description);

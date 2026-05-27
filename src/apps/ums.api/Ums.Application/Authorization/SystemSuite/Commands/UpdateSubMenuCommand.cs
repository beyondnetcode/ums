namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record UpdateSubMenuCommand(
    Guid SystemSuiteId,
    Guid ModuleId,
    Guid MenuId,
    Guid SubMenuId,
    string Label,
    string Description,
    int SortOrder) : ICommand;

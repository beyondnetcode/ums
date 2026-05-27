namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record AddOptionCommand(
    Guid SystemSuiteId,
    Guid ModuleId,
    Guid MenuId,
    Guid SubMenuId,
    string Code,
    string Label,
    string Description,
    string ActionCode,
    int SortOrder) : ICommand;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record AddSubMenuCommand(
    Guid SystemSuiteId,
    Guid ModuleId,
    Guid MenuId,
    string Code,
    string Label,
    string Description,
    int SortOrder) : ICommand;

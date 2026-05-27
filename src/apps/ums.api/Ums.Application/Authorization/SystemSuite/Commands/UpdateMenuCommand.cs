namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record UpdateMenuCommand(
    Guid SystemSuiteId,
    Guid ModuleId,
    Guid MenuId,
    string Label,
    string Description,
    int SortOrder) : ICommand;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record AddMenuCommand(
    Guid SystemSuiteId,
    Guid ModuleId,
    string Code,
    string Label,
    string Description,
    int SortOrder) : ICommand;

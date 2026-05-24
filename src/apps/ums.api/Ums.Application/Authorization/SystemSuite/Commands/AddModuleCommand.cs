namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record AddModuleCommand(
    Guid SystemSuiteId,
    string Code,
    string Name,
    string Description,
    int SortOrder) : ICommand;

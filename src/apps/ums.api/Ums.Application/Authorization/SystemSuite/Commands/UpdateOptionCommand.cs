namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record UpdateOptionCommand(
    Guid SystemSuiteId,
    Guid ModuleId,
    Guid MenuId,
    Guid SubMenuId,
    Guid OptionId,
    string Label,
    string Description,
    string ActionCode,
    int SortOrder) : ICommand;

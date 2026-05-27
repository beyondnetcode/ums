namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record RemoveOptionCommand(Guid SystemSuiteId, Guid ModuleId, Guid MenuId, Guid SubMenuId, Guid OptionId) : ICommand;

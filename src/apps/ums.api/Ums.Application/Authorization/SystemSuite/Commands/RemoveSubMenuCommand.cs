namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record RemoveSubMenuCommand(Guid SystemSuiteId, Guid ModuleId, Guid MenuId, Guid SubMenuId) : ICommand;

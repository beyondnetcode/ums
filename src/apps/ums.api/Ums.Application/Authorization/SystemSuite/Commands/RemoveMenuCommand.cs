namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record RemoveMenuCommand(Guid SystemSuiteId, Guid ModuleId, Guid MenuId) : ICommand;

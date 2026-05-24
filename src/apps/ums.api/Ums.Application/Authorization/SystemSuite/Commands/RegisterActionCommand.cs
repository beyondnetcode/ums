namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record RegisterActionCommand(Guid SystemSuiteId, string Code, string Name) : ICommand;

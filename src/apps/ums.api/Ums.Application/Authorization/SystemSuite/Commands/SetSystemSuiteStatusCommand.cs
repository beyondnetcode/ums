using Ums.Application.Authorization.SystemSuite.DTOs;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record SetSystemSuiteStatusCommand(Guid SystemSuiteId, string Status) : ICommand;

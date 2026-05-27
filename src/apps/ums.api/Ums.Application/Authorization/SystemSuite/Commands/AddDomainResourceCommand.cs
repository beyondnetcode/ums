using System;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record AddDomainResourceCommand(
    Guid SystemSuiteId,
    Guid? ModuleId,
    string Type,
    string Code,
    string Name,
    string Description) : ICommand;

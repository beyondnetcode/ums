using System;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record AddDomainResourceCommand(
    Guid SystemSuiteId,
    Guid? ModuleId,
    Guid? ParentResourceId,
    string Type,
    string Code,
    string Name,
    string Description) : ICommand;

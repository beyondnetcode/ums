using System;

namespace Ums.Application.Authorization.SystemSuite.Commands;

public sealed record RemoveDomainResourceCommand(
    Guid SystemSuiteId,
    Guid DomainResourceId) : ICommand;

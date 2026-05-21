using Ums.Application.Authorization.SystemSuite.DTOs;

namespace Ums.Application.Authorization.SystemSuite.Queries;

public sealed record GetSystemSuiteByIdQuery(Guid SystemSuiteId) : IQuery<SystemSuiteDto>;

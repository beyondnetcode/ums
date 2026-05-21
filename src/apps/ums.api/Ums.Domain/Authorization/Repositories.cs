namespace Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.Template;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;

public interface IProfileRepository : IAggregateRepository<ProfileAggregate>
{
    Task<ProfileAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ISystemSuiteRepository : IAggregateRepository<SystemSuiteAggregate>
{
    Task<SystemSuiteAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SystemSuiteAggregate?> GetByCodeAsync(Code code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSuiteAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSuiteAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IPermissionTemplateRepository : IAggregateRepository<PermissionTemplateAggregate>
{
    Task<PermissionTemplateAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionTemplateAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PermissionTemplateAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

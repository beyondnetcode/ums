namespace Ums.Domain.Authorization;
using Ums.Domain.Authorization.Profile;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Authorization.Role;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;
using RoleAggregate = Ums.Domain.Authorization.Role.Role;

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
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRoleRepository : IAggregateRepository<RoleAggregate>
{
    Task<RoleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleAggregate?> GetByCodeAsync(Guid systemSuiteId, Code code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleAggregate>> GetBySystemSuiteIdAsync(Guid systemSuiteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

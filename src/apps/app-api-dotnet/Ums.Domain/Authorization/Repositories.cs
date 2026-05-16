namespace Ums.Domain.Authorization;

using Ums.Domain.Kernel;

public interface ISystemSuiteRepository : ITenantScopedRepository<SystemSuite>
{
    Task<SystemSuite?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
}

public interface IRoleRepository : ITenantScopedRepository<Role>
{
    Task<Role?> GetByCodeAsync(Guid tenantId, Guid systemSuiteId, string code, CancellationToken cancellationToken = default);
}

public interface IPermissionTemplateRepository : ITenantScopedRepository<PermissionTemplate>
{
    Task<PermissionTemplate?> GetByCodeAsync(Guid tenantId, Guid systemSuiteId, string code, string version, CancellationToken cancellationToken = default);
}

public interface IProfileRepository : ITenantScopedRepository<Profile>
{
    Task<IReadOnlyCollection<Profile>> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

public interface ITemplateAssignmentRuleRepository : ITenantScopedRepository<TemplateAssignmentRule>
{
    Task<IReadOnlyCollection<TemplateAssignmentRule>> GetActiveByTemplateAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default);
}

public interface ICompiledPolicyGraphRepository
{
    Task<CompiledPolicyGraph?> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
    Task SaveAsync(CompiledPolicyGraph graph, CancellationToken cancellationToken = default);
}

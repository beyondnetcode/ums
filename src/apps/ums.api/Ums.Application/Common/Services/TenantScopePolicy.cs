using Ums.Domain.Identity;

namespace Ums.Application.Common.Interfaces;

/// <summary>
/// Default tenant scope policy used by portal management flows.
/// It resolves the visible tenant scope and validates management-owner access
/// for operations that can modify UMS configuration.
/// </summary>
public sealed class TenantScopePolicy : ITenantScopePolicy
{
    private readonly ITenantContext _tenantContext;
    private readonly IUserContext _userContext;
    private readonly ITenantRepository _tenantRepository;

    public TenantScopePolicy(
        ITenantContext tenantContext,
        IUserContext userContext,
        ITenantRepository tenantRepository)
    {
        _tenantContext = tenantContext;
        _userContext = userContext;
        _tenantRepository = tenantRepository;
    }

    public Guid? ResolveQueryScope()
    {
        if (_tenantContext.IsInternalAdmin)
        {
            return _tenantContext.OrganizationId;
        }

        if (_tenantContext.OrganizationId.HasValue)
        {
            return _tenantContext.OrganizationId;
        }

        return Guid.TryParse(_userContext.TenantId, out var tenantId) ? tenantId : null;
    }

    public async Task<Result> EnsureManagementOwnerScopeAsync(Guid targetTenantId, CancellationToken cancellationToken = default)
    {
        var currentTenantId = ResolveCurrentTenantId();
        if (currentTenantId is null)
        {
            return Result.Failure("AUTH_013: Tenant context is required for management access.");
        }

        if (currentTenantId.Value != targetTenantId)
        {
            return Result.Failure(
                $"AUTH_014: Tenant mismatch. User belongs to tenant '{currentTenantId}', but request targets '{targetTenantId}'.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(targetTenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure("AUTH_002: Tenant not found.");
        }

        if (!tenant.IsManagementOwner)
        {
            return Result.Failure("AUTH_015: Tenant is not marked as management owner.");
        }

        return Result.Success();
    }

    private Guid? ResolveCurrentTenantId()
    {
        if (_tenantContext.OrganizationId.HasValue)
        {
            return _tenantContext.OrganizationId;
        }

        return Guid.TryParse(_userContext.TenantId, out var tenantId) ? tenantId : null;
    }
}

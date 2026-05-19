namespace Ums.Application.Common.Interfaces;

/// <summary>
/// Domain-agnostic port to access the current tenant/organization context.
/// Implemented by the Infrastructure layer via Scoped lifecycle.
/// </summary>
public interface ITenantContext
{
    Guid? OrganizationId { get; }
    void SetOrganizationId(Guid organizationId);
}

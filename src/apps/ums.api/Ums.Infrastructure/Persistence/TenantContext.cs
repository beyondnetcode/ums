
namespace Ums.Infrastructure.Persistence;

/// <summary>
/// Concrete implementation of the tenant container.
/// Registered with Scoped lifecycle in the DI container.
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid? OrganizationId { get; private set; }

    public void SetOrganizationId(Guid organizationId)
    {
        if (OrganizationId.HasValue && OrganizationId.Value != organizationId)
        {
            throw new InvalidOperationException("Cannot redefine an established Organization Context within the same request lifecycle.");
        }

        OrganizationId = organizationId;
    }
}

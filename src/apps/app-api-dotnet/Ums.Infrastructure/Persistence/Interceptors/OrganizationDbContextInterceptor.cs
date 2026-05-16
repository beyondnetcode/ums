using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ums.Application.Common.Interfaces;

namespace Ums.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that sets the organization context in the SQL Server session.
/// Application-layer tenant filtering remains the primary isolation mechanism; this context supports database RLS as a failsafe.
/// </summary>
public class OrganizationDbContextInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;

    public OrganizationDbContextInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Sets the SQL Server session context immediately after the connection is opened.
    /// </summary>
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (_tenantContext.OrganizationId.HasValue)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "EXEC sp_set_session_context @key = N'current_organization_id', @value = @organizationId;";

            var organizationId = command.CreateParameter();
            organizationId.ParameterName = "@organizationId";
            organizationId.Value = _tenantContext.OrganizationId.Value;
            command.Parameters.Add(organizationId);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        if (_tenantContext.OrganizationId.HasValue)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "EXEC sp_set_session_context @key = N'current_organization_id', @value = @organizationId;";

            var organizationId = command.CreateParameter();
            organizationId.ParameterName = "@organizationId";
            organizationId.Value = _tenantContext.OrganizationId.Value;
            command.Parameters.Add(organizationId);

            command.ExecuteNonQuery();
        }

        base.ConnectionOpened(connection, eventData);
    }
}

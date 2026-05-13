using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ums.Application.Common.Interfaces;

namespace Ums.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core Interceptor that automatically injects the Organization Context 
/// into the PostgreSQL session before any command is executed.
/// This enables Row-Level Security (RLS) enforcement at the database engine level.
/// </summary>
public class OrganizationDbContextInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;

    public OrganizationDbContextInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Executes native SQL to set the session variable immediately after connection is opened.
    /// Uses 'SET LOCAL' so the setting is restricted to the current transaction.
    /// </summary>
    public override async ValueTask<InterceptionResult> ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (_tenantContext.OrganizationId.HasValue)
        {
            var orgId = _tenantContext.OrganizationId.Value.ToString();
            
            using var command = connection.CreateCommand();
            command.CommandText = $"SET LOCAL app.current_organization_id = '{orgId}';";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return await base.ConnectionOpenedAsync(connection, eventData, result, cancellationToken);
    }

    public override InterceptionResult ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData,
        InterceptionResult result)
    {
        if (_tenantContext.OrganizationId.HasValue)
        {
            var orgId = _tenantContext.OrganizationId.Value.ToString();
            
            using var command = connection.CreateCommand();
            command.CommandText = $"SET LOCAL app.current_organization_id = '{orgId}';";
            command.ExecuteNonQuery();
        }

        return base.ConnectionOpened(connection, eventData, result);
    }
}

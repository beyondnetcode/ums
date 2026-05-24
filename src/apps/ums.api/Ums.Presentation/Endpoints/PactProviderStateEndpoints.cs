using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Enums;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence;

namespace Ums.Presentation.Endpoints;

/// <summary>
/// OPS-02: Pact provider state endpoint.
///
/// Mounted at /_pact/provider-states (Development environment only).
/// PactNet's verifier calls this endpoint before each interaction to set up the
/// preconditions declared in the consumer pact ("Given(…)").
///
/// Security: guarded by environment check in Program.cs — never registered in Production.
/// </summary>
public static class PactProviderStateEndpoints
{
    public static IEndpointRouteBuilder MapPactProviderStateEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/_pact/provider-states", async (
            ProviderStateRequest   request,
            ITenantRepository      tenants,
            IUserAccountRepository userAccounts) =>
        {
            // Each "Given" clause in a consumer test maps to a state name here.
            await (request.State switch
            {
                var s when s.StartsWith("a tenant with id ")       => EnsureTenantExistsAsync(s, tenants),
                var s when s.StartsWith("no tenant with id ")      => Task.CompletedTask,
                "at least one tenant exists"                        => EnsureDefaultTenantAsync(tenants),
                var s when s.StartsWith("a user account with id ") => EnsureUserAccountExistsAsync(s, userAccounts, tenants),
                var s when s.StartsWith("no user account with id ")=> Task.CompletedTask,
                "at least one user account exists"                  => EnsureDefaultUserAccountAsync(userAccounts, tenants),
                _                                                   => Task.CompletedTask,
            });

            return Results.Ok();
        }).WithTags("Pact").ExcludeFromDescription();  // Hidden from Swagger docs.

        return app;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Constants
    // ──────────────────────────────────────────────────────────────────────────

    private static readonly Guid DefaultTenantGuid = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private static readonly Guid DefaultUserGuid   = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private static readonly ActorId SeedActor      = ActorId.Create("00000000-0000-0000-0000-000000000111");

    // ──────────────────────────────────────────────────────────────────────────
    // State helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static async Task EnsureDefaultTenantAsync(ITenantRepository tenants)
    {
        if (await tenants.GetByIdAsync(DefaultTenantGuid) is not null) return;
        await SeedTenantAsync(DefaultTenantGuid, "ACME", "Acme Corp", tenants);
    }

    private static async Task EnsureTenantExistsAsync(string state, ITenantRepository tenants)
    {
        // Parse id from "a tenant with id {guid} exists"
        var parts = state.Split(' ');
        if (!Guid.TryParse(parts[^2], out var id)) return;
        if (await tenants.GetByIdAsync(id) is not null) return;
        await SeedTenantAsync(id, "ACME", "Acme Corp", tenants);
    }

    private static async Task SeedTenantAsync(Guid id, string code, string name, ITenantRepository tenants)
    {
        var result = Tenant.Create(
            Code.Create(code),
            Name.Create(name),
            OrganizationType.INTERNAL,
            SeedActor,
            IdpStrategy.InternalBcrypt,
            companyReference: null,
            parentTenantId:   null,
            tenantId:         TenantId.Load(id));

        if (result.IsFailure) return;

        var tenant = result.Value;
        tenant.Activate(SeedActor);

        await tenants.AddAsync(tenant);
        await tenants.UnitOfWork.SaveEntitiesAsync();
    }

    private static async Task EnsureDefaultUserAccountAsync(
        IUserAccountRepository userAccounts, ITenantRepository tenants)
    {
        await EnsureDefaultTenantAsync(tenants);
        if (await userAccounts.GetByIdAsync(DefaultUserGuid) is not null) return;
        await SeedUserAccountAsync(DefaultUserGuid, DefaultTenantGuid, "user@example.com", userAccounts);
    }

    private static async Task EnsureUserAccountExistsAsync(
        string state, IUserAccountRepository userAccounts, ITenantRepository tenants)
    {
        var parts = state.Split(' ');
        if (!Guid.TryParse(parts[^2], out var id)) return;
        await EnsureDefaultTenantAsync(tenants);
        if (await userAccounts.GetByIdAsync(id) is not null) return;
        await SeedUserAccountAsync(id, DefaultTenantGuid, "user@example.com", userAccounts);
    }

    private static async Task SeedUserAccountAsync(
        Guid id, Guid tenantId, string email, IUserAccountRepository userAccounts)
    {
        var result = UserAccount.Create(
            TenantId.Load(tenantId),
            Email.Create(email),
            UserCategory.Internal,
            identityReference:     null,
            identityReferenceType: null,
            SeedActor,
            branchId:      null,
            userAccountId: UserAccountId.Load(id));

        if (result.IsFailure) return;

        var account = result.Value;
        account.Activate(SeedActor);

        await userAccounts.AddAsync(account);
        await userAccounts.UnitOfWork.SaveEntitiesAsync();
    }
}

/// <summary>Body shape posted by PactNet verifier to set up provider state.</summary>
public sealed record ProviderStateRequest(string State, Dictionary<string, string>? Params = null);

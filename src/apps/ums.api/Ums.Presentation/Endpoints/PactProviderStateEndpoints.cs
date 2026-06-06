using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Ums.Domain.Enums;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.SystemSuite;
using Ums.Domain.Authorization.Template;
using Ums.Domain.Authorization.Profile;
using Ums.Infrastructure.Persistence;

namespace Ums.Presentation.Endpoints;

/// <summary>
/// OPS-02: Pact provider state endpoint.
/// </summary>
public static class PactProviderStateEndpoints
{
    public static IEndpointRouteBuilder MapPactProviderStateEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/_pact/provider-states", async (
            ProviderStateRequest request,
            ITenantRepository      tenants,
            IUserAccountRepository userAccounts,
            IPermissionTemplateRepository permissionTemplates,
            IProfileRepository profiles,
            ISystemSuiteRepository systemSuites) =>
        {
            await (request.State switch
            {
                var s when s.StartsWith("a tenant with id ")       => EnsureTenantExistsAsync(s, tenants),
                var s when s.StartsWith("no tenant with id ")      => Task.CompletedTask,
                "at least one tenant exists"                        => EnsureDefaultTenantAsync(tenants),
                
                var s when s.StartsWith("a user account with id ") && s.EndsWith(" is active") => EnsureUserAccountExistsAsync(s, userAccounts, tenants, true),
                var s when s.StartsWith("a user account with id ") && s.EndsWith(" is inactive") => EnsureUserAccountExistsAsync(s, userAccounts, tenants, false),
                var s when s.StartsWith("a user account with id ") => EnsureUserAccountExistsAsync(s, userAccounts, tenants, true),
                var s when s.StartsWith("no user account with id ")=> Task.CompletedTask,
                "at least one user account exists"                  => EnsureDefaultUserAccountAsync(userAccounts, tenants),
                "a user account exists with valid credentials"      => EnsureDefaultUserAccountWithPasswordAsync(userAccounts, tenants),
                "a user account does not exist or credentials do not match" => Task.CompletedTask,

                var s when s.StartsWith("a permission template with id ") => EnsurePermissionTemplateExistsAsync(s, permissionTemplates, tenants, systemSuites),
                "at least one permission template exists"           => EnsureDefaultPermissionTemplateAsync(permissionTemplates, tenants, systemSuites),

                var s when s.StartsWith("a profile with id ")      => EnsureProfileExistsAsync(s, profiles, tenants, userAccounts),
                "at least one profile exists"                       => EnsureDefaultProfileAsync(profiles, tenants, userAccounts),

                var s when s.StartsWith("a system suite with id ") => EnsureSystemSuiteExistsAsync(s, systemSuites, tenants),
                "at least one system suite exists"                  => EnsureDefaultSystemSuiteAsync(systemSuites, tenants),

                _                                                   => Task.CompletedTask,
            });

            return Results.Ok();
        }).WithTags("Pact").ExcludeFromDescription().AllowAnonymous();

        return app;
    }

    private static readonly Guid DefaultTenantGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DefaultUserGuid   = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private static readonly ActorId SeedActor      = ActorId.Create("00000000-0000-0000-0000-000000000111");

    private static void SetEntityId<T>(T entity, Guid id) where T : class
    {
        var type = entity.GetType();
        var propsProperty = type.BaseType?.GetProperty("Props", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (propsProperty != null)
        {
            var props = propsProperty.GetValue(entity);
            if (props != null)
            {
                var idProperty = props.GetType().GetProperty("Id");
                if (idProperty != null)
                {
                    var idType = idProperty.PropertyType;
                    var loadMethod = idType.GetMethod("Load", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, new[] { typeof(Guid) });
                    if (loadMethod != null)
                    {
                        var idVal = loadMethod.Invoke(null, new object[] { id });
                        idProperty.SetValue(props, idVal);
                    }
                }
            }
        }
    }

    private static async Task EnsureDefaultTenantAsync(ITenantRepository tenants)
    {
        if (await tenants.GetByIdAsync(DefaultTenantGuid) is not null) return;
        await SeedTenantAsync(DefaultTenantGuid, "ACME", "Acme Corp", tenants);
    }

    private static async Task EnsureTenantExistsAsync(string state, ITenantRepository tenants)
    {
        var id = ExtractGuid(state);
        if (id == Guid.Empty) return;
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

    private static async Task EnsureDefaultUserAccountWithPasswordAsync(
        IUserAccountRepository userAccounts, ITenantRepository tenants)
    {
        await EnsureDefaultUserAccountAsync(userAccounts, tenants);
        var user = await userAccounts.GetByIdAsync(DefaultUserGuid);
        if (user != null)
        {
            // Bcrypt hash for "ValidPassword123!"
            user.AddPassword(PasswordHash.Create("$2a$11$00000000000000000000001Z9KzDqM8G.Y3kG.0Hw8z.gLdD/W5qG"), SeedActor);
            await userAccounts.UnitOfWork.SaveEntitiesAsync();
        }
    }

    private static async Task EnsureDefaultUserAccountAsync(
        IUserAccountRepository userAccounts, ITenantRepository tenants)
    {
        await EnsureDefaultTenantAsync(tenants);
        if (await userAccounts.GetByIdAsync(DefaultUserGuid) is not null) return;
        await SeedUserAccountAsync(DefaultUserGuid, DefaultTenantGuid, "user@example.com", userAccounts, true);
    }

    private static async Task EnsureUserAccountExistsAsync(
        string state, IUserAccountRepository userAccounts, ITenantRepository tenants, bool isActive)
    {
        var id = ExtractGuid(state);
        if (id == Guid.Empty) return;
        await EnsureDefaultTenantAsync(tenants);
        if (await userAccounts.GetByIdAsync(id) is not null) return;
        await SeedUserAccountAsync(id, DefaultTenantGuid, $"pact-test-{id}@contract.test", userAccounts, isActive);
    }

    private static async Task SeedUserAccountAsync(
        Guid id, Guid tenantId, string email, IUserAccountRepository userAccounts, bool isActive = true)
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
        if (isActive)
        {
            account.Activate(SeedActor);
        }

        await userAccounts.AddAsync(account);
        await userAccounts.UnitOfWork.SaveEntitiesAsync();
    }

    private static async Task EnsureDefaultSystemSuiteAsync(ISystemSuiteRepository systemSuites, ITenantRepository tenants)
    {
        await EnsureDefaultTenantAsync(tenants);
        var id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        if (await systemSuites.GetByIdAsync(id) is not null) return;
        await SeedSystemSuiteAsync(id, DefaultTenantGuid, systemSuites);
    }

    private static async Task EnsureSystemSuiteExistsAsync(string state, ISystemSuiteRepository systemSuites, ITenantRepository tenants)
    {
        await EnsureDefaultTenantAsync(tenants);
        var id = ExtractGuid(state);
        if (id == Guid.Empty) return;
        if (await systemSuites.GetByIdAsync(id) is not null) return;
        await SeedSystemSuiteAsync(id, DefaultTenantGuid, systemSuites);
    }

    private static async Task SeedSystemSuiteAsync(Guid id, Guid tenantId, ISystemSuiteRepository systemSuites)
    {
        var result = SystemSuite.Create(
            TenantId.Load(tenantId),
            Code.Create("UMS-CORE"),
            Name.Create("User Management System"),
            Description.Create("Core System"),
            SeedActor);
        if (result.IsFailure) return;

        var suite = result.Value;
        SetEntityId(suite, id);
        
        // Use reflection if Activate method is internal or missing
        var activateMethod = suite.GetType().GetMethod("Activate");
        activateMethod?.Invoke(suite, new object[] { SeedActor });

        await systemSuites.AddAsync(suite);
        await systemSuites.UnitOfWork.SaveEntitiesAsync();
    }

    private static async Task EnsureDefaultPermissionTemplateAsync(IPermissionTemplateRepository permissionTemplates, ITenantRepository tenants, ISystemSuiteRepository systemSuites)
    {
        await EnsureDefaultSystemSuiteAsync(systemSuites, tenants);
        var id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        if (await permissionTemplates.GetByIdAsync(id) is not null) return;
        await SeedPermissionTemplateAsync(id, DefaultTenantGuid, Guid.NewGuid(), id, permissionTemplates);
    }

    private static async Task EnsurePermissionTemplateExistsAsync(string state, IPermissionTemplateRepository permissionTemplates, ITenantRepository tenants, ISystemSuiteRepository systemSuites)
    {
        await EnsureDefaultSystemSuiteAsync(systemSuites, tenants);
        var id = ExtractGuid(state);
        if (id == Guid.Empty) return;
        if (await permissionTemplates.GetByIdAsync(id) is not null) return;
        await SeedPermissionTemplateAsync(id, DefaultTenantGuid, Guid.NewGuid(), id, permissionTemplates);
    }

    private static async Task SeedPermissionTemplateAsync(Guid id, Guid tenantId, Guid roleId, Guid suiteId, IPermissionTemplateRepository permissionTemplates)
    {
        var result = PermissionTemplate.Create(
            TenantId.Load(tenantId),
            RoleId.Load(roleId),
            SystemSuiteId.Load(suiteId),
            SeedActor);
        if (result.IsFailure) return;

        var template = result.Value;
        SetEntityId(template, id);
        
        var publishMethod = template.GetType().GetMethod("Publish");
        publishMethod?.Invoke(template, new object[] { SeedActor });

        await permissionTemplates.AddAsync(template);
        await permissionTemplates.UnitOfWork.SaveEntitiesAsync();
    }

    private static async Task EnsureDefaultProfileAsync(IProfileRepository profiles, ITenantRepository tenants, IUserAccountRepository userAccounts)
    {
        await EnsureDefaultUserAccountAsync(userAccounts, tenants);
        var id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        if (await profiles.GetByIdAsync(id) is not null) return;
        await SeedProfileAsync(id, DefaultTenantGuid, DefaultUserGuid, Guid.NewGuid(), profiles);
    }

    private static async Task EnsureProfileExistsAsync(string state, IProfileRepository profiles, ITenantRepository tenants, IUserAccountRepository userAccounts)
    {
        await EnsureDefaultUserAccountAsync(userAccounts, tenants);
        var id = ExtractGuid(state);
        if (id == Guid.Empty) return;
        if (await profiles.GetByIdAsync(id) is not null) return;
        await SeedProfileAsync(id, DefaultTenantGuid, DefaultUserGuid, Guid.NewGuid(), profiles);
    }

    private static async Task SeedProfileAsync(Guid id, Guid tenantId, Guid userId, Guid roleId, IProfileRepository profiles)
    {
        var result = Profile.Create(
            TenantId.Load(tenantId),
            UserId.Load(userId),
            RoleId.Load(roleId),
            null,
            SeedActor);
        if (result.IsFailure) return;

        var profile = result.Value;
        SetEntityId(profile, id);
        
        var activateMethod = profile.GetType().GetMethod("Activate");
        activateMethod?.Invoke(profile, new object[] { SeedActor });

        await profiles.AddAsync(profile);
        await profiles.UnitOfWork.SaveEntitiesAsync();
    }

    private static Guid ExtractGuid(string state)
    {
        foreach (var part in state.Split(' '))
        {
            if (Guid.TryParse(part, out var guid))
            {
                return guid;
            }
        }
        return Guid.Empty;
    }
}

/// <summary>Body shape posted by PactNet verifier to set up provider state.</summary>
public sealed record ProviderStateRequest(string State, Dictionary<string, string>? Params = null);

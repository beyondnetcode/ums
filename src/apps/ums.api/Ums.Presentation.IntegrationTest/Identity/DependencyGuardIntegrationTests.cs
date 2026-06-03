using Ums.Presentation.IntegrationTest.Infrastructure;
using Ums.Infrastructure.Persistence.Seeders;

namespace Ums.Presentation.IntegrationTest.Identity;

/// <summary>
/// Integration tests for the dependency guard policy (ADR-0079).
/// Verifies that state-change operations blocked by active dependencies return HTTP 409
/// with a structured <c>BlockedOperationResponse</c> payload.
/// </summary>
public sealed class DependencyGuardIntegrationTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DependencyGuardIntegrationTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        _client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000123");
        _client.DefaultRequestHeaders.Add("X-User-Name", "Integration Tester");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", CoreDevDataSeeder.InternalAdminTenantId);
    }

    [Fact]
    public async Task SuspendTenant_WithActiveUsers_ShouldReturn409WithBlockingDependencies()
    {
        // Arrange: create a fresh tenant
        var tenantCode = $"DG_{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var createTenantResponse = await _client.PostAsJsonAsync("/api/v1/tenants", new
        {
            code = tenantCode,
            name = "Dependency Guard Test Tenant",
            ruc = $"20{new Random().Next(100000000, 999999999)}",
        }, TestContext.Current.CancellationToken);

        createTenantResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var tenantPayload = JsonDocument.Parse(await createTenantResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var newTenantId = tenantPayload.RootElement.GetProperty("tenantId").GetGuid();

        // Activate the tenant
        var activateTenantResponse = await _client.PostAsync(
            $"/api/v1/tenants/{newTenantId}/activate", null, TestContext.Current.CancellationToken);
        activateTenantResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Create and activate a user in this tenant
        var email = $"guard.user.{Guid.NewGuid():N}@ums.local";
        var createUserResponse = await _client.PostAsJsonAsync("/api/v1/user-accounts", new
        {
            tenantId = newTenantId,
            branchId = (Guid?)null,
            email,
            category = "Internal",
            identityReference = $"EMP-{Guid.NewGuid():N}"[..10],
            identityReferenceType = "HrId",
        }, TestContext.Current.CancellationToken);

        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var userPayload = JsonDocument.Parse(await createUserResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var userId = userPayload.RootElement.GetProperty("userAccountId").GetGuid();

        await _client.PostAsync($"/api/v1/user-accounts/{userId}/activate", null, TestContext.Current.CancellationToken);

        // Act: attempt to suspend the tenant
        var suspendResponse = await _client.PostAsync(
            $"/api/v1/tenants/{newTenantId}/suspend", null, TestContext.Current.CancellationToken);

        // Assert: 409 Conflict with structured blocking dependencies
        suspendResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var errorPayload = JsonDocument.Parse(await suspendResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        errorPayload.RootElement.GetProperty("errorCode").GetString().Should().NotBeNullOrWhiteSpace();
        errorPayload.RootElement.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        errorPayload.RootElement.GetProperty("brokenRule").GetString().Should().NotBeNullOrWhiteSpace();

        var deps = errorPayload.RootElement.GetProperty("blockingDependencies");
        deps.GetArrayLength().Should().BeGreaterThan(0);

        var firstDep = deps[0];
        firstDep.GetProperty("entityType").GetString().Should().NotBeNullOrWhiteSpace();
        firstDep.GetProperty("count").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task BlockUser_WithActiveProfiles_ShouldReturn409WithBlockingDependencies()
    {
        var tenantId = Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId);

        // Create user and activate them
        var email = $"guard.profile.{Guid.NewGuid():N}@ums.local";
        var createUserResponse = await _client.PostAsJsonAsync("/api/v1/user-accounts", new
        {
            tenantId,
            branchId = (Guid?)null,
            email,
            category = "Internal",
            identityReference = $"EMP-{Guid.NewGuid():N}"[..10],
            identityReferenceType = "HrId",
        }, TestContext.Current.CancellationToken);

        createUserResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var userPayload = JsonDocument.Parse(await createUserResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var userId = userPayload.RootElement.GetProperty("userAccountId").GetGuid();

        await _client.PostAsync($"/api/v1/user-accounts/{userId}/activate", null, TestContext.Current.CancellationToken);

        // Assign an active profile to the user
        var systemSuiteId = await GetManagementSystemSuiteIdAsync(tenantId, TestContext.Current.CancellationToken);
        var roleId = await CreateRoleAsync(systemSuiteId, TestContext.Current.CancellationToken);
        var branchId = await CreateBranchAsync(tenantId, TestContext.Current.CancellationToken);

        var createProfileResponse = await _client.PostAsJsonAsync("/api/v1/profiles", new
        {
            tenantId,
            userId,
            roleId,
            branchId,
        }, TestContext.Current.CancellationToken);

        createProfileResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act: attempt to block the user while they have an active profile
        var blockResponse = await _client.PostAsync(
            $"/api/v1/user-accounts/{userId}/block?reason=dependency-guard-test", null, TestContext.Current.CancellationToken);

        // Assert: 409 Conflict
        blockResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var errorPayload = JsonDocument.Parse(await blockResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        errorPayload.RootElement.GetProperty("errorCode").GetString().Should().NotBeNullOrWhiteSpace();
        errorPayload.RootElement.GetProperty("blockingDependencies").GetArrayLength().Should().BeGreaterThan(0);

        var firstDep = errorPayload.RootElement.GetProperty("blockingDependencies")[0];
        firstDep.GetProperty("entityType").GetString().Should().Be("Profile");
    }

    private async Task<Guid> GetManagementSystemSuiteIdAsync(Guid tenantId, CancellationToken ct)
    {
        var response = await _client.GetAsync($"/api/v1/system-suites?page=1&pageSize=20&tenantId={tenantId}", ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        foreach (var item in payload.RootElement.GetProperty("items").EnumerateArray())
        {
            if (string.Equals(item.GetProperty("code").GetString(), "UMS", StringComparison.OrdinalIgnoreCase))
                return item.GetProperty("systemSuiteId").GetGuid();
        }

        throw new InvalidOperationException("Seeded UMS system suite not found.");
    }

    private async Task<Guid> CreateRoleAsync(Guid systemSuiteId, CancellationToken ct)
    {
        var code = $"DG_ROLE_{Guid.NewGuid():N}"[..20];
        var response = await _client.PostAsJsonAsync($"/api/v1/system-suites/{systemSuiteId}/roles", new
        {
            code,
            value = "Dependency Guard Role",
            description = "Temporary role for dependency guard tests.",
            parentRoleId = (Guid?)null,
            hierarchyLevel = 0,
            promotionOrder = 0,
        }, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return payload.RootElement.GetProperty("roleId").GetGuid();
    }

    private async Task<Guid> CreateBranchAsync(Guid tenantId, CancellationToken ct)
    {
        var code = $"DG_{Guid.NewGuid():N}"[..12];
        var response = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", new
        {
            code,
            name = "Dependency Guard Branch",
            geofencingMetadata = (string?)null,
        }, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return payload.RootElement.GetProperty("branchId").GetGuid();
    }
}

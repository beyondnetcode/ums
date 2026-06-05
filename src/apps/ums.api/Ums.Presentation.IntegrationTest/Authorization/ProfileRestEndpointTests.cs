using Ums.Presentation.IntegrationTest.Infrastructure;
using Ums.Infrastructure.Persistence.Seeders;

namespace Ums.Presentation.IntegrationTest.Authorization;

public sealed class ProfileRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProfileRestEndpointTests(UmsApiWebApplicationFactory factory)
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
    public async Task CreateProfile_Deactivate_Activate_ShouldSucceed()
    {
        var tenantId = Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId);
        var systemSuiteId = await GetManagementSystemSuiteIdAsync(tenantId, TestContext.Current.CancellationToken);
        var roleId = await CreateRoleAsync(systemSuiteId, TestContext.Current.CancellationToken);
        var branchId = await CreateBranchAsync(tenantId, TestContext.Current.CancellationToken);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/profiles", new
        {
            tenantId,
            userId = Guid.NewGuid(),
            roleId,
            branchId,
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createdPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var profileId = createdPayload.RootElement.GetProperty("profileId").GetGuid();

        var getCreatedResponse = await _client.GetAsync($"/api/v1/profiles/{profileId}", TestContext.Current.CancellationToken);
        getCreatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var createdProfilePayload = JsonDocument.Parse(await getCreatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        createdProfilePayload.RootElement.GetProperty("profileId").GetGuid().Should().Be(profileId);
        createdProfilePayload.RootElement.GetProperty("scope").GetString().Should().Be("BranchScoped");
        createdProfilePayload.RootElement.GetProperty("isActive").GetBoolean().Should().BeTrue();

        var deactivateResponse = await _client.PostAsync($"/api/v1/profiles/{profileId}/deactivate", null, TestContext.Current.CancellationToken);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeactivatedResponse = await _client.GetAsync($"/api/v1/profiles/{profileId}", TestContext.Current.CancellationToken);
        using var deactivatedPayload = JsonDocument.Parse(await getDeactivatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        deactivatedPayload.RootElement.GetProperty("isActive").GetBoolean().Should().BeFalse();

        var activateResponse = await _client.PostAsync($"/api/v1/profiles/{profileId}/activate", null, TestContext.Current.CancellationToken);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getActivatedResponse = await _client.GetAsync($"/api/v1/profiles/{profileId}", TestContext.Current.CancellationToken);
        using var activatedPayload = JsonDocument.Parse(await getActivatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        activatedPayload.RootElement.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileById_WhenMissing_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/profiles/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAuthGraphPreview_WithValidProfile_ShouldReturn200WithPreviewPayload()
    {
        var tenantId = Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId);
        var systemSuiteId = await GetManagementSystemSuiteIdAsync(tenantId, TestContext.Current.CancellationToken);
        var roleId = await CreateRoleAsync(systemSuiteId, TestContext.Current.CancellationToken);
        var branchId = await CreateBranchAsync(tenantId, TestContext.Current.CancellationToken);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/profiles", new
        {
            tenantId,
            userId = Guid.NewGuid(),
            roleId,
            branchId,
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createdPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var profileId = createdPayload.RootElement.GetProperty("profileId").GetGuid();

        var previewResponse = await _client.GetAsync(
            $"/api/v1/profiles/{profileId}/auth-graph/preview",
            TestContext.Current.CancellationToken);

        previewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        previewResponse.Headers.Should().ContainKey("X-Preview-Mode");
        previewResponse.Headers.GetValues("X-Preview-Mode").Should().ContainSingle("internal-preview");
        previewResponse.Headers.Should().ContainKey("X-Request-Id");

        using var payload = JsonDocument.Parse(await previewResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.GetProperty("previewMode").GetString().Should().Be("internal-preview");
        payload.RootElement.GetProperty("profileId").GetGuid().Should().Be(profileId);
        payload.RootElement.GetProperty("tenantId").GetGuid().Should().Be(tenantId);
        payload.RootElement.GetProperty("requestId").GetString().Should().NotBeNullOrWhiteSpace();
        payload.RootElement.GetProperty("format").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetAuthGraphPreview_WhenProfileIsMissing_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/v1/profiles/{Guid.NewGuid()}/auth-graph/preview?format=JSON",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.GetProperty("error").GetString().Should().Be("Profile not found.");
        payload.RootElement.GetProperty("errorId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAuthGraphPreview_WithoutAuthentication_ShouldReturn401()
    {
        var unauthClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost"),
        };

        var response = await unauthClient.GetAsync(
            $"/api/v1/profiles/{Guid.NewGuid()}/auth-graph/preview",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<Guid> GetManagementSystemSuiteIdAsync(Guid tenantId, CancellationToken ct)
    {
        var response = await _client.GetAsync($"/api/v1/system-suites?page=1&pageSize=20&tenantId={tenantId}", ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var items = payload.RootElement.GetProperty("items");

        foreach (var item in items.EnumerateArray())
        {
            if (string.Equals(item.GetProperty("code").GetString(), "UMS", StringComparison.OrdinalIgnoreCase))
            {
                return item.GetProperty("systemSuiteId").GetGuid();
            }
        }

        throw new InvalidOperationException("Seeded UMS system suite was not found for the internal admin tenant.");
    }

    private async Task<Guid> CreateRoleAsync(Guid systemSuiteId, CancellationToken ct)
    {
        var roleCode = $"INTEGRATION_ADMIN_{Guid.NewGuid():N}"[..24];
        var response = await _client.PostAsJsonAsync($"/api/v1/system-suites/{systemSuiteId}/roles", new
        {
            code = roleCode,
            value = "Integration Admin",
            description = "Temporary role used by the profile REST integration test.",
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
        var branchCode = $"INT_{Guid.NewGuid():N}"[..12];
        var response = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", new
        {
            code = branchCode,
            name = "Integration Branch",
            geofencingMetadata = (string?)null,
        }, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return payload.RootElement.GetProperty("branchId").GetGuid();
    }
}

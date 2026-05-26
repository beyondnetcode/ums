using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.E2E;

/// <summary>
/// E2E tests for the Tenant bounded context.
/// Covers full CRUD lifecycle against a real SQL Server Testcontainer:
///   - Create / Get by ID / Get list with pagination + search
///   - Status transitions: Activate / Suspend
///   - Branch sub-resource: Add / Deactivate / Reactivate / Remove
///   - Validation errors (400), Not-Found (404), Conflict (409)
///
/// Prerequisites: Docker must be running locally.
/// Tests are automatically skipped when Docker is unavailable.
/// </summary>
[Collection("SqlServer")]
public sealed class TenantE2ETests
{
    private readonly SqlServerContainerFixture _fixture;
    private readonly HttpClient _client;

    public TenantE2ETests(SqlServerContainerFixture fixture)
    {
        _fixture = fixture;

        if (fixture.IsAvailable)
        {
            var factory = new SqlServerWebApplicationFactory(fixture.ConnectionString);
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
            });
            _client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000001");
            _client.DefaultRequestHeaders.Add("X-User-Name", "e2e-test");
        }
        else
        {
            _client = new HttpClient();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTenant_ValidPayload_Returns201WithLocation()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync("/api/v1/tenants", NewTenantPayload(), ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        doc.RootElement.GetProperty("tenantId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTenant_MissingCode_Returns400()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var payload = new { code = "", name = "Missing Code Corp", type = "Customer", idpStrategy = (string?)null, companyReference = (string?)null };
        var response = await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTenant_DuplicateCode_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var payload = NewTenantPayload();
        var first = await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/api/v1/tenants", payload with { Name = "Duplicate Org" }, ct);
        second.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTenantById_ExistingTenant_Returns200WithCorrectData()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var payload = NewTenantPayload();
        var createRes = await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenantId = await ReadGuidProperty(createRes, "tenantId", ct);

        var getRes = await _client.GetAsync($"/api/v1/tenants/{tenantId}", ct);

        getRes.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await getRes.Content.ReadAsStringAsync(ct));
        doc.RootElement.GetProperty("tenantId").GetGuid().Should().Be(tenantId);
        doc.RootElement.GetProperty("code").GetString().Should().Be(payload.Code);
        doc.RootElement.GetProperty("name").GetString().Should().Be(payload.Name);
        doc.RootElement.GetProperty("type").GetString().Should().Be(payload.Type);
        doc.RootElement.GetProperty("status").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetTenantById_NonExistent_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var res = await _client.GetAsync($"/api/v1/tenants/{Guid.NewGuid()}", ct);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTenants_Pagination_ReturnsCorrectMetadata()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var res = await _client.GetAsync("/api/v1/tenants?page=1&pageSize=5", ct);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        doc.RootElement.GetProperty("page").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(5);
        doc.RootElement.GetProperty("totalItems").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        doc.RootElement.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetTenants_SearchByCode_FindsCreatedTenant()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var payload = NewTenantPayload();
        (await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var res = await _client.GetAsync($"/api/v1/tenants?page=1&pageSize=50&search={payload.Code}&criteria=code", ct);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        var items = doc.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        var found = Enumerable.Range(0, items.GetArrayLength())
            .Any(i => items[i].GetProperty("code").GetString() == payload.Code);
        found.Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STATUS TRANSITIONS
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SuspendAndActivateTenant_FullCycle_Returns204()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantAndGetId(ct);

        // Suspend
        var suspendRes = await _client.PostAsync($"/api/v1/tenants/{tenantId}/suspend", null, ct);
        suspendRes.StatusCode.Should().Be(HttpStatusCode.NoContent,
            because: "a newly created tenant should be suspendable");

        // Verify status
        var suspended = await GetTenantDoc(tenantId, ct);
        suspended.RootElement.GetProperty("status").GetString().Should().Be("Suspended");
        suspended.Dispose();

        // Activate
        var activateRes = await _client.PostAsync($"/api/v1/tenants/{tenantId}/activate", null, ct);
        activateRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify restored status
        var activated = await GetTenantDoc(tenantId, ct);
        activated.RootElement.GetProperty("status").GetString().Should().Be("Active");
        activated.Dispose();
    }

    [Fact]
    public async Task SuspendTenant_AlreadySuspended_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantAndGetId(ct);
        await _client.PostAsync($"/api/v1/tenants/{tenantId}/suspend", null, ct);

        var second = await _client.PostAsync($"/api/v1/tenants/{tenantId}/suspend", null, ct);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task SuspendTenant_NonExistent_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var res = await _client.PostAsync($"/api/v1/tenants/{Guid.NewGuid()}/suspend", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateTenant_NonExistent_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var res = await _client.PostAsync($"/api/v1/tenants/{Guid.NewGuid()}/activate", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BRANCH sub-resource
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddBranch_ValidPayload_Returns201()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantAndGetId(ct);
        var branchPayload = new { code = $"BR{Guid.NewGuid():N}"[..8], name = "Main Branch", geofencingMetadata = (string?)null };

        var res = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct);

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        doc.RootElement.GetProperty("tenantId").GetGuid().Should().Be(tenantId);
    }

    [Fact]
    public async Task AddBranch_ToNonExistentTenant_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var branchPayload = new { code = "BR-GHOST", name = "Ghost Branch", geofencingMetadata = (string?)null };
        var res = await _client.PostAsJsonAsync($"/api/v1/tenants/{Guid.NewGuid()}/branches", branchPayload, ct);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BranchLifecycle_DeactivateReactivateRemove_FullCycle()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantAndGetId(ct);
        var branchPayload = new { code = $"LF{Guid.NewGuid():N}"[..8], name = "Lifecycle Branch", geofencingMetadata = (string?)null };
        var addRes = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct);
        addRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var branchId = await ReadGuidProperty(addRes, "branchId", ct);

        // Deactivate
        var deactivate = await _client.PostAsync($"/api/v1/tenants/{tenantId}/branches/{branchId}/deactivate", null, ct);
        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Reactivate
        var reactivate = await _client.PostAsync($"/api/v1/tenants/{tenantId}/branches/{branchId}/reactivate", null, ct);
        reactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Remove
        var remove = await _client.DeleteAsync($"/api/v1/tenants/{tenantId}/branches/{branchId}", ct);
        remove.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddBranch_DuplicateCode_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantAndGetId(ct);
        var branchPayload = new { code = $"DUP{Guid.NewGuid():N}"[..8], name = "Dupe Branch", geofencingMetadata = (string?)null };

        (await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct);
        second.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static (string Code, string Name, string Type, string? IdpStrategy, string? CompanyReference) NewTenantPayload()
    {
        var uid = Guid.NewGuid().ToString("N")[..10].ToUpper();
        return (Code: $"T{uid}", Name: $"E2E Tenant {uid}", Type: "Customer", IdpStrategy: null, CompanyReference: null);
    }

    private async Task<Guid> CreateTenantAndGetId(CancellationToken ct)
    {
        var res = await _client.PostAsJsonAsync("/api/v1/tenants", NewTenantPayload(), ct);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadGuidProperty(res, "tenantId", ct);
    }

    private async Task<JsonDocument> GetTenantDoc(Guid tenantId, CancellationToken ct)
    {
        var res = await _client.GetAsync($"/api/v1/tenants/{tenantId}", ct);
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        return JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
    }

    private static async Task<Guid> ReadGuidProperty(HttpResponseMessage response, string property, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty(property).GetGuid();
    }
}

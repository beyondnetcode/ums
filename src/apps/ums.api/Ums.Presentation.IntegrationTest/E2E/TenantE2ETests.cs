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
/// Architecture:
///   - Commands  → REST API  (POST / PUT / DELETE)
///   - Queries   → GraphQL   (POST /graphql)
///
/// Prerequisites: Docker must be running locally.
/// Tests are automatically skipped when Docker is unavailable.
/// </summary>
[Collection("PostgreSql")]
public sealed class TenantE2ETests
{
    private readonly PostgreSqlContainerFixture _fixture;
    private readonly HttpClient _client;

    public TenantE2ETests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;

        if (fixture.IsAvailable)
        {
            var factory = new PostgreSqlWebApplicationFactory(fixture.ConnectionString);
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

        var payload = new { code = "", name = "Missing Code Corp", type = "CLIENT", idpStrategy = (string?)null, companyReference = (string?)null };
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
        second.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ — via GraphQL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTenantById_ExistingTenant_GqlReturnsCorrectData()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var payload = NewTenantPayload();
        var createRes = await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenantId = await ReadGuidProperty(createRes, "tenantId", ct);

        using var doc = await GqlTenantByIdAsync(tenantId, ct);
        var tenant = doc.RootElement.GetProperty("data").GetProperty("tenantById");

        tenant.ValueKind.Should().NotBe(JsonValueKind.Null, because: "tenant should exist");
        tenant.GetProperty("tenantId").GetGuid().Should().Be(tenantId);
        tenant.GetProperty("code").GetString().Should().Be(payload.Code);
        tenant.GetProperty("name").GetString().Should().Be(payload.Name);
        tenant.GetProperty("type").GetString().Should().Be(payload.Type);
        tenant.GetProperty("status").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetTenantById_NonExistent_GqlReturnsNull()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        using var doc = await GqlTenantByIdAsync(Guid.NewGuid(), ct);
        var tenant = doc.RootElement.GetProperty("data").GetProperty("tenantById");

        tenant.ValueKind.Should().Be(JsonValueKind.Null,
            because: "querying a non-existent tenant ID should return null");
    }

    [Fact]
    public async Task GetTenants_Pagination_GqlReturnsCorrectMetadata()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        const string gql = "{ tenants(page: 1, pageSize: 5) { page pageSize totalItems items { tenantId code } } }";
        using var doc = await GqlQueryAsync(gql, ct);

        var list = doc.RootElement.GetProperty("data").GetProperty("tenants");
        list.GetProperty("page").GetInt32().Should().Be(1);
        list.GetProperty("pageSize").GetInt32().Should().Be(5);
        list.GetProperty("totalItems").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        list.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetTenants_SearchByCode_GqlFindsTenant()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var payload = NewTenantPayload();
        (await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var gql = $"{{ tenants(page: 1, pageSize: 50, search: \"{payload.Code}\", criteria: \"code\") {{ items {{ tenantId code }} }} }}";
        using var doc = await GqlQueryAsync(gql, ct);

        var items = doc.RootElement.GetProperty("data").GetProperty("tenants").GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        var found = items.EnumerateArray().Any(i => i.GetProperty("code").GetString() == payload.Code);
        found.Should().BeTrue(because: "tenant should be findable by exact code search");
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

        // Verify status via GraphQL
        using (var afterSuspend = await GqlTenantByIdAsync(tenantId, ct))
        {
            afterSuspend.RootElement.GetProperty("data").GetProperty("tenantById")
                .GetProperty("status").GetString().Should().Be("Suspended");
        }

        // Activate
        var activateRes = await _client.PostAsync($"/api/v1/tenants/{tenantId}/activate", null, ct);
        activateRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify restored status via GraphQL
        using (var afterActivate = await GqlTenantByIdAsync(tenantId, ct))
        {
            afterActivate.RootElement.GetProperty("data").GetProperty("tenantById")
                .GetProperty("status").GetString().Should().Be("Active");
        }
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
    public async Task AddBranch_ValidPayload_Returns201AndAppearsInGqlBranches()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantAndGetId(ct);
        var branchCode = $"BR{Guid.NewGuid():N}"[..8];
        var branchPayload = new { code = branchCode, name = "Main Branch", geofencingMetadata = (string?)null };

        var res = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify via GraphQL tenantBranches query
        var gql = $"{{ tenantBranches(tenantId: \"{tenantId}\") {{ branchId code name isActive }} }}";
        using var doc = await GqlQueryAsync(gql, ct);
        var branches = doc.RootElement.GetProperty("data").GetProperty("tenantBranches");
        var found = branches.EnumerateArray().Any(b => b.GetProperty("code").GetString() == branchCode);
        found.Should().BeTrue(because: "the added branch should appear in tenantBranches query");
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
        var branchCode = $"LF{Guid.NewGuid():N}"[..8];
        var branchPayload = new { code = branchCode, name = "Lifecycle Branch", geofencingMetadata = (string?)null };

        var addRes = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct);
        addRes.StatusCode.Should().Be(HttpStatusCode.Created);

        // Retrieve branchId via GraphQL (AddBranchResponse only contains tenantId)
        var branchId = await GetBranchIdByCodeAsync(tenantId, branchCode, ct);

        // Deactivate
        (await _client.PostAsync($"/api/v1/tenants/{tenantId}/branches/{branchId}/deactivate", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Reactivate
        (await _client.PostAsync($"/api/v1/tenants/{tenantId}/branches/{branchId}/reactivate", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Remove
        (await _client.DeleteAsync($"/api/v1/tenants/{tenantId}/branches/{branchId}", ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify branch is gone via GraphQL
        var gql = $"{{ tenantBranches(tenantId: \"{tenantId}\") {{ branchId code }} }}";
        using var after = await GqlQueryAsync(gql, ct);
        var branches = after.RootElement.GetProperty("data").GetProperty("tenantBranches");
        var stillPresent = branches.EnumerateArray().Any(b => b.GetProperty("branchId").GetGuid() == branchId);
        stillPresent.Should().BeFalse(because: "removed branch should not appear in branches list");
    }

    [Fact]
    public async Task AddBranch_DuplicateCode_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantAndGetId(ct);
        var branchCode = $"DUP{Guid.NewGuid():N}"[..8];
        var branchPayload = new { code = branchCode, name = "Dupe Branch", geofencingMetadata = (string?)null };

        (await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync($"/api/v1/tenants/{tenantId}/branches", branchPayload, ct);
        second.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Sends a raw GraphQL query to POST /graphql and returns the parsed response.</summary>
    private async Task<JsonDocument> GqlQueryAsync(string gql, CancellationToken ct)
    {
        var res = await _client.PostAsJsonAsync("/graphql", new { query = gql }, ct);
        res.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        doc.RootElement.TryGetProperty("errors", out _).Should().BeFalse(
            because: "GraphQL query should not return errors");
        return doc;
    }

    /// <summary>Queries tenantById(tenantId) via GraphQL. Caller must dispose.</summary>
    private Task<JsonDocument> GqlTenantByIdAsync(Guid tenantId, CancellationToken ct) =>
        GqlQueryAsync(
            $"{{ tenantById(tenantId: \"{tenantId}\") {{ tenantId code name type status }} }}",
            ct);

    /// <summary>
    /// Retrieves the branchId for a given tenant+code via GraphQL.
    /// Needed because AddBranchResponse only contains tenantId.
    /// </summary>
    private async Task<Guid> GetBranchIdByCodeAsync(Guid tenantId, string code, CancellationToken ct)
    {
        var gql = $"{{ tenantBranches(tenantId: \"{tenantId}\") {{ branchId code }} }}";
        using var doc = await GqlQueryAsync(gql, ct);
        var branches = doc.RootElement.GetProperty("data").GetProperty("tenantBranches");
        return branches.EnumerateArray()
            .First(b => b.GetProperty("code").GetString() == code)
            .GetProperty("branchId").GetGuid();
    }

    private record TenantPayload(string Code, string Name, string Type, string? IdpStrategy, string? CompanyReference, bool IsManagementOwner = false);
    private static TenantPayload NewTenantPayload()
    {
        var uid = Guid.NewGuid().ToString("N")[..10].ToUpper();
        return new TenantPayload($"T{uid}", $"E2E Tenant {uid}", "CLIENT", null, null, true);
    }

    private async Task<Guid> CreateTenantAndGetId(CancellationToken ct)
    {
        var res = await _client.PostAsJsonAsync("/api/v1/tenants", NewTenantPayload(), ct);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var location = res.Headers.Location?.ToString();
        var idString = location!.Split('/').Last();
        var id = Guid.Parse(idString);

        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", id.ToString());

        return id;
    }

    private static async Task<Guid> ReadGuidProperty(HttpResponseMessage response, string property, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty(property).GetGuid();
    }
}

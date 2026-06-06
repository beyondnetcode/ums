using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.E2E;

/// <summary>
/// E2E tests for the SystemSuite bounded context (Authorization).
/// Covers full CRUD lifecycle against a real SQL Server Testcontainer:
///   - Create / Get by ID / Get list with pagination + search
///   - Update suite metadata
///   - Status management: SetStatus (Active / Inactive)
///   - Module sub-resource: Add / Update / Deactivate / Activate / Remove
///   - AppSetting sub-resource: Add / Update / Remove
///   - Action sub-resource: Register / Remove
///   - Validation errors (400), Not-Found (404), Conflict (409)
///
/// Architecture:
///   - Commands  → REST API  (POST / PUT / DELETE)
///   - Queries   → GraphQL   (POST /graphql)
///
/// Each test creates its own Tenant + SystemSuite to guarantee isolation.
/// Prerequisites: Docker must be running locally.
/// </summary>
[Collection("SqlServer")]
public sealed class SystemSuiteE2ETests
{
    private readonly SqlServerContainerFixture _fixture;
    private readonly HttpClient _client;

    public SystemSuiteE2ETests(SqlServerContainerFixture fixture)
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
    public async Task CreateSystemSuite_ValidPayload_Returns201WithId()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var response = await _client.PostAsJsonAsync("/api/v1/system-suites", NewSuitePayload(tenantId), ct);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        doc.RootElement.GetProperty("systemSuiteId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateSystemSuite_MissingName_Returns400()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = new { tenantId, code = UniqueCode("SS"), name = "", description = "No name" };

        var res = await _client.PostAsJsonAsync("/api/v1/system-suites", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSystemSuite_DuplicateCode_SameTenant_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = NewSuitePayload(tenantId);
        (await _client.PostAsJsonAsync("/api/v1/system-suites", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var dup = await _client.PostAsJsonAsync("/api/v1/system-suites", payload with { Name = "Duplicate Suite" }, ct);
        dup.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ — via GraphQL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSystemSuiteById_ExistingSuite_GqlReturnsCorrectFields()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = NewSuitePayload(tenantId);
        var createRes = await _client.PostAsJsonAsync("/api/v1/system-suites", payload, ct);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var suiteId = await ReadGuid(createRes, "systemSuiteId", ct);

        using var doc = await GqlSuiteByIdAsync(suiteId, ct);
        var suite = doc.RootElement.GetProperty("data").GetProperty("systemSuiteById");

        suite.ValueKind.Should().NotBe(JsonValueKind.Null, because: "suite should exist");
        suite.GetProperty("systemSuiteId").GetGuid().Should().Be(suiteId);
        suite.GetProperty("tenantId").GetGuid().Should().Be(tenantId);
        suite.GetProperty("code").GetString().Should().Be(payload.Code);
        suite.GetProperty("name").GetString().Should().Be(payload.Name);
        suite.GetProperty("status").GetString().Should().NotBeNullOrWhiteSpace();
        suite.GetProperty("modules").ValueKind.Should().Be(JsonValueKind.Array);
        suite.GetProperty("actions").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetSystemSuiteById_NonExistent_GqlReturnsNull()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        using var doc = await GqlSuiteByIdAsync(Guid.NewGuid(), ct);
        var suite = doc.RootElement.GetProperty("data").GetProperty("systemSuiteById");

        suite.ValueKind.Should().Be(JsonValueKind.Null,
            because: "querying a non-existent suite ID should return null");
    }

    [Fact]
    public async Task GetSystemSuites_Pagination_GqlReturnsPageMetadata()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        const string gql = "{ systemSuites(page: 1, pageSize: 5) { page pageSize totalItems items { systemSuiteId code } } }";
        using var doc = await GqlQueryAsync(gql, ct);

        var list = doc.RootElement.GetProperty("data").GetProperty("systemSuites");
        list.GetProperty("page").GetInt32().Should().Be(1);
        list.GetProperty("pageSize").GetInt32().Should().Be(5);
        list.GetProperty("totalItems").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        list.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetSystemSuites_FilterByTenantId_GqlOnlyReturnsTenantSuites()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        (await _client.PostAsJsonAsync("/api/v1/system-suites", NewSuitePayload(tenantId), ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var gql = $"{{ systemSuites(page: 1, pageSize: 50, tenantId: \"{tenantId}\") {{ items {{ systemSuiteId tenantId code }} }} }}";
        using var doc = await GqlQueryAsync(gql, ct);

        var items = doc.RootElement.GetProperty("data").GetProperty("systemSuites").GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        foreach (var item in items.EnumerateArray())
        {
            item.GetProperty("tenantId").GetGuid().Should().Be(tenantId,
                because: "tenant filter should only return suites for that tenant");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSystemSuite_ValidPayload_Returns204AndPersistsChanges()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var updatePayload = new { systemSuiteId = suiteId, name = "Updated Suite Name", description = "Updated description for E2E test" };

        var updateRes = await _client.PutAsJsonAsync($"/api/v1/system-suites/{suiteId}", updatePayload, ct);
        updateRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlSuiteByIdAsync(suiteId, ct);
        var suite = doc.RootElement.GetProperty("data").GetProperty("systemSuiteById");
        suite.GetProperty("name").GetString().Should().Be("Updated Suite Name");
        suite.GetProperty("description").GetString().Should().Be("Updated description for E2E test");
    }

    [Fact]
    public async Task UpdateSystemSuite_NonExistent_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var ghost = Guid.NewGuid();
        var payload = new { systemSuiteId = ghost, name = "Ghost", description = "Ghost suite" };

        var res = await _client.PutAsJsonAsync($"/api/v1/system-suites/{ghost}", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STATUS
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetSystemSuiteStatus_Inactive_Returns204AndPersistsStatus()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);

        var res = await _client.PostAsync($"/api/v1/system-suites/{suiteId}/status?status=Inactive", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlSuiteByIdAsync(suiteId, ct);
        doc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
            .GetProperty("status").GetString().Should().Be("Inactive");
    }

    [Fact]
    public async Task SetSystemSuiteStatus_BackToActive_Returns204()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        await _client.PostAsync($"/api/v1/system-suites/{suiteId}/status?status=Inactive", null, ct);

        var res = await _client.PostAsync($"/api/v1/system-suites/{suiteId}/status?status=Active", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlSuiteByIdAsync(suiteId, ct);
        doc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
            .GetProperty("status").GetString().Should().Be("Active");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MODULE sub-resource
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddModule_ValidPayload_Returns204AndAppearsInSuite()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var moduleCode = UniqueCode("MOD");
        var payload = new { systemSuiteId = suiteId, code = moduleCode, name = "Users Module", description = "Manages user data", sortOrder = 1 };

        var res = await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/modules", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlSuiteByIdAsync(suiteId, ct);
        var modules = doc.RootElement.GetProperty("data").GetProperty("systemSuiteById").GetProperty("modules");
        var found = modules.EnumerateArray().Any(m => m.GetProperty("code").GetString() == moduleCode);
        found.Should().BeTrue(because: "the module should appear in the suite after being added");
    }

    [Fact]
    public async Task UpdateModule_ValidPayload_Returns204AndPersistsChanges()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var moduleCode = UniqueCode("UM");
        await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/modules",
            new { systemSuiteId = suiteId, code = moduleCode, name = "Original", description = "Desc", sortOrder = 1 }, ct);

        // Get moduleId via GraphQL
        using var beforeDoc = await GqlSuiteByIdAsync(suiteId, ct);
        var moduleId = beforeDoc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
            .GetProperty("modules").EnumerateArray()
            .First(m => m.GetProperty("code").GetString() == moduleCode)
            .GetProperty("id").GetGuid();

        var updatePayload = new { systemSuiteId = suiteId, moduleId, name = "Updated Module", description = "New description", sortOrder = 2 };
        var res = await _client.PutAsJsonAsync($"/api/v1/system-suites/{suiteId}/modules/{moduleId}", updatePayload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var afterDoc = await GqlSuiteByIdAsync(suiteId, ct);
        var module = afterDoc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
            .GetProperty("modules").EnumerateArray()
            .First(m => m.GetProperty("id").GetGuid() == moduleId);
        module.GetProperty("name").GetString().Should().Be("Updated Module");
        module.GetProperty("sortOrder").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task ModuleLifecycle_DeactivateActivateRemove_FullCycle()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var code = UniqueCode("LC");
        await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/modules",
            new { systemSuiteId = suiteId, code, name = "Lifecycle Module", description = "E2E lifecycle", sortOrder = 5 }, ct);

        // Get moduleId via GraphQL
        using var addedDoc = await GqlSuiteByIdAsync(suiteId, ct);
        var moduleId = addedDoc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
            .GetProperty("modules").EnumerateArray()
            .First(m => m.GetProperty("code").GetString() == code)
            .GetProperty("id").GetGuid();

        // Deactivate
        (await _client.PostAsync($"/api/v1/system-suites/{suiteId}/modules/{moduleId}/deactivate", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deactivated via GraphQL
        using (var deactivatedDoc = await GqlSuiteByIdAsync(suiteId, ct))
        {
            deactivatedDoc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
                .GetProperty("modules").EnumerateArray()
                .First(m => m.GetProperty("id").GetGuid() == moduleId)
                .GetProperty("status").GetString().Should().Be("Inactive");
        }

        // Activate
        (await _client.PostAsync($"/api/v1/system-suites/{suiteId}/modules/{moduleId}/activate", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Deactivate again before remove (remove only works on inactive modules)
        (await _client.PostAsync($"/api/v1/system-suites/{suiteId}/modules/{moduleId}/deactivate", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Remove
        (await _client.DeleteAsync($"/api/v1/system-suites/{suiteId}/modules/{moduleId}", ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify gone via GraphQL
        using var afterDoc = await GqlSuiteByIdAsync(suiteId, ct);
        var still = afterDoc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
            .GetProperty("modules").EnumerateArray()
            .Any(m => m.GetProperty("id").GetGuid() == moduleId);
        still.Should().BeFalse(because: "removed module should not appear in suite");
    }

    [Fact]
    public async Task AddModule_DuplicateCode_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var code = UniqueCode("DUPM");
        var payload = new { systemSuiteId = suiteId, code, name = "Module 1", description = "Dup test", sortOrder = 1 };

        (await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/modules", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dup = await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/modules", payload, ct);
        dup.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // APP SETTING sub-resource
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAppSetting_ValidPayload_Returns204()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var payload = new { systemSuiteId = suiteId, key = $"feature.{UniqueCode("k")}", value = "true", scope = "Tenant" };

        var res = await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateAppSetting_ExistingKey_Returns204()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var key = $"setting.{UniqueCode("s")}";
        await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings",
            new { systemSuiteId = suiteId, key, value = "initial", scope = "Tenant" }, ct);

        var payload = new { systemSuiteId = suiteId, key, value = "updated" };
        var res = await _client.PutAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings/{key}", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveAppSetting_ExistingKey_Returns204()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var key = $"del.{UniqueCode("d")}";
        await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings",
            new { systemSuiteId = suiteId, key, value = "to_delete", scope = "Global" }, ct);

        var res = await _client.DeleteAsync($"/api/v1/system-suites/{suiteId}/app-settings/{key}", ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AppSettingFullCycle_Add_Update_Remove()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var key = $"full.cycle.{UniqueCode("fc")}";

        // Add
        (await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings",
            new { systemSuiteId = suiteId, key, value = "v1", scope = "Module" }, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Update
        (await _client.PutAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings/{key}",
            new { systemSuiteId = suiteId, key, value = "v2" }, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Remove
        (await _client.DeleteAsync($"/api/v1/system-suites/{suiteId}/app-settings/{key}", ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddAppSetting_DuplicateKey_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var key = $"dup.key.{UniqueCode("dk")}";
        var payload = new { systemSuiteId = suiteId, key, value = "v1", scope = "Tenant" };

        (await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dup = await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/app-settings", payload, ct);
        dup.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ACTION sub-resource
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAction_ValidPayload_Returns204AndAppearsInSuite()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var code = UniqueCode("ACT");
        var payload = new { systemSuiteId = suiteId, code, name = "Can Export Reports" };

        var res = await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/actions", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlSuiteByIdAsync(suiteId, ct);
        var actions = doc.RootElement.GetProperty("data").GetProperty("systemSuiteById").GetProperty("actions");
        actions.EnumerateArray().Any(a => a.GetProperty("code").GetString() == code).Should().BeTrue(
            because: "registered action should appear in suite actions");
    }

    [Fact]
    public async Task RemoveAction_ExistingAction_Returns204AndDisappearsFromSuite()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var code = UniqueCode("DEL");
        await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/actions",
            new { systemSuiteId = suiteId, code, name = "Deletable Action" }, ct);

        var res = await _client.DeleteAsync($"/api/v1/system-suites/{suiteId}/actions/{code}", ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlSuiteByIdAsync(suiteId, ct);
        doc.RootElement.GetProperty("data").GetProperty("systemSuiteById")
            .GetProperty("actions").EnumerateArray()
            .Any(a => a.GetProperty("code").GetString() == code).Should().BeFalse(
                because: "removed action should not appear in suite");
    }

    [Fact]
    public async Task RegisterAction_DuplicateCode_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var code = UniqueCode("DUPACT");
        var payload = new { systemSuiteId = suiteId, code, name = "Duplicate Action" };

        (await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/actions", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dup = await _client.PostAsJsonAsync($"/api/v1/system-suites/{suiteId}/actions", payload, ct);
        dup.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveAction_NonExistentCode_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var suiteId = await CreateSuiteId(ct);
        var res = await _client.DeleteAsync($"/api/v1/system-suites/{suiteId}/actions/GHOST_CODE", ct);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

    /// <summary>
    /// Queries systemSuiteById(systemSuiteId) with full fields via GraphQL. Caller must dispose.
    /// </summary>
    private Task<JsonDocument> GqlSuiteByIdAsync(Guid suiteId, CancellationToken ct) =>
        GqlQueryAsync(
            $"{{ systemSuiteById(systemSuiteId: \"{suiteId}\") {{ systemSuiteId tenantId code name description status modules {{ id code name description status sortOrder }} actions {{ id code name }} }} }}",
            ct);

    private static string UniqueCode(string prefix)
        => $"{prefix}{Guid.NewGuid():N}"[..Math.Min(20, prefix.Length + 32)];

    private record SuitePayload(Guid TenantId, string Code, string Name, string Description);
    private static SuitePayload NewSuitePayload(Guid tenantId)
    {
        var uid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return new SuitePayload(tenantId, $"SS{uid}", $"E2E Suite {uid}", "Created by E2E test suite");
    }

    private async Task<Guid> CreateTenantId(CancellationToken ct)
    {
        var uid = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var payload = new { code = $"T{uid}", name = $"E2E SS Tenant {uid}", type = "CLIENT", idpStrategy = (string?)null, companyReference = (string?)null, isManagementOwner = true };
        var response = await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        var idString = location!.Split('/').Last();
        var id = Guid.Parse(idString);

        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", id.ToString());

        return id;
    }

    private async Task<Guid> CreateSuiteId(CancellationToken ct)
    {
        var tenantId = await CreateTenantId(ct);
        var res = await _client.PostAsJsonAsync("/api/v1/system-suites", NewSuitePayload(tenantId), ct);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadGuid(res, "systemSuiteId", ct);
    }

    private static async Task<Guid> ReadGuid(HttpResponseMessage response, string property, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty(property).GetGuid();
    }
}

using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.E2E;

/// <summary>
/// E2E tests for the UserAccount bounded context.
/// Covers full CRUD lifecycle against a real SQL Server Testcontainer:
///   - Create / Get by ID / Get list with pagination, search, and tenant filter
///   - Status transitions: Activate / Block (with reason) / Restore
///   - Soft-delete (GDPR anonymization)
///   - Authentication attempt recording (audit trail)
///   - Validation errors (400), Not-Found (404), Conflict (409)
///
/// Architecture:
///   - Commands  → REST API  (POST / PUT / DELETE)
///   - Queries   → GraphQL   (POST /graphql)
///
/// Each test creates its own Tenant to guarantee isolation.
/// Prerequisites: Docker must be running locally.
/// </summary>
[Collection("SqlServer")]
public sealed class UserAccountE2ETests
{
    private readonly SqlServerContainerFixture _fixture;
    private readonly HttpClient _client;

    public UserAccountE2ETests(SqlServerContainerFixture fixture)
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
    public async Task CreateUserAccount_ValidPayload_Returns201WithId()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var response = await _client.PostAsJsonAsync("/api/v1/user-accounts", NewUserPayload(tenantId), ct);

        var content = await response.Content.ReadAsStringAsync(ct);
        System.IO.File.WriteAllText("e2e_error.txt", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created, content);
        response.Headers.Location.Should().NotBeNull();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        doc.RootElement.GetProperty("userAccountId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUserAccount_InvalidEmail_Returns400()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = new { tenantId, branchId = (Guid?)null, email = "NOT_AN_EMAIL", category = "Internal", identityReference = (string?)null, identityReferenceType = (string?)null };

        var res = await _client.PostAsJsonAsync("/api/v1/user-accounts", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserAccount_DuplicateEmail_SameTenant_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = NewUserPayload(tenantId);

        (await _client.PostAsJsonAsync("/api/v1/user-accounts", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var dup = await _client.PostAsJsonAsync("/api/v1/user-accounts", payload, ct);
        dup.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // READ — via GraphQL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserAccountById_ExistingAccount_GqlReturnsCorrectFields()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = NewUserPayload(tenantId);
        var createRes = await _client.PostAsJsonAsync("/api/v1/user-accounts", payload, ct);
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var userId = await ReadGuid(createRes, "userAccountId", ct);

        using var doc = await GqlUserAccountByIdAsync(userId, ct);
        var user = doc.RootElement.GetProperty("data").GetProperty("userAccountById");

        user.ValueKind.Should().NotBe(JsonValueKind.Null, because: "user account should exist");
        user.GetProperty("userAccountId").GetGuid().Should().Be(userId);
        user.GetProperty("tenantId").GetGuid().Should().Be(tenantId);
        user.GetProperty("email").GetString().Should().Be(payload.Email);
        user.GetProperty("category").GetString().Should().Be(payload.Category);
        user.GetProperty("status").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetUserAccountById_NonExistent_GqlReturnsNull()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        using var doc = await GqlUserAccountByIdAsync(Guid.NewGuid(), ct);
        var user = doc.RootElement.GetProperty("data").GetProperty("userAccountById");

        user.ValueKind.Should().Be(JsonValueKind.Null,
            because: "querying a non-existent user account ID should return null");
    }

    [Fact]
    public async Task GetUserAccounts_Pagination_GqlReturnsPageMetadata()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        const string gql = "{ userAccounts(page: 1, pageSize: 5) { page pageSize totalItems items { userAccountId email } } }";
        using var doc = await GqlQueryAsync(gql, ct);

        var list = doc.RootElement.GetProperty("data").GetProperty("userAccounts");
        list.GetProperty("page").GetInt32().Should().Be(1);
        list.GetProperty("pageSize").GetInt32().Should().Be(5);
        list.GetProperty("totalItems").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        list.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetUserAccounts_FilterByTenantId_GqlReturnsOnlyTenantUsers()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = NewUserPayload(tenantId);
        (await _client.PostAsJsonAsync("/api/v1/user-accounts", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var gql = $"{{ userAccounts(page: 1, pageSize: 50, tenantId: \"{tenantId}\") {{ items {{ userAccountId tenantId email }} }} }}";
        using var doc = await GqlQueryAsync(gql, ct);

        var items = doc.RootElement.GetProperty("data").GetProperty("userAccounts").GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        foreach (var item in items.EnumerateArray())
        {
            item.GetProperty("tenantId").GetGuid().Should().Be(tenantId,
                because: "tenant filter should only return users of that tenant");
        }
    }

    [Fact]
    public async Task GetUserAccounts_SearchByEmail_GqlFindsCreatedUser()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var tenantId = await CreateTenantId(ct);
        var payload = NewUserPayload(tenantId);
        (await _client.PostAsJsonAsync("/api/v1/user-accounts", payload, ct))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var emailPrefix = payload.Email.Split('@')[0];
        var gql = $"{{ userAccounts(page: 1, pageSize: 50, search: \"{emailPrefix}\", criteria: \"email\") {{ items {{ userAccountId email }} }} }}";
        using var doc = await GqlQueryAsync(gql, ct);

        var items = doc.RootElement.GetProperty("data").GetProperty("userAccounts").GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        var found = items.EnumerateArray().Any(i => i.GetProperty("email").GetString() == payload.Email);
        found.Should().BeTrue(because: "user should be findable by email prefix search");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STATUS TRANSITIONS
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ActivateUserAccount_FromPending_Returns204AndChangesStatus()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);

        var res = await _client.PostAsync($"/api/v1/user-accounts/{userId}/activate", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlUserAccountByIdAsync(userId, ct);
        doc.RootElement.GetProperty("data").GetProperty("userAccountById")
            .GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task BlockUserAccount_ActiveAccount_Returns204AndChangesStatus()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);
        (await _client.PostAsync($"/api/v1/user-accounts/{userId}/activate", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var res = await _client.PostAsync($"/api/v1/user-accounts/{userId}/block?reason=Policy+violation", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlUserAccountByIdAsync(userId, ct);
        doc.RootElement.GetProperty("data").GetProperty("userAccountById")
            .GetProperty("status").GetString().Should().Be("Blocked");
    }

    [Fact]
    public async Task RestoreUserAccount_BlockedAccount_Returns204AndChangesStatus()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);
        await _client.PostAsync($"/api/v1/user-accounts/{userId}/activate", null, ct);
        await _client.PostAsync($"/api/v1/user-accounts/{userId}/block?reason=Test+block", null, ct);

        var res = await _client.PostAsync($"/api/v1/user-accounts/{userId}/restore", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var doc = await GqlUserAccountByIdAsync(userId, ct);
        doc.RootElement.GetProperty("data").GetProperty("userAccountById")
            .GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task FullStatusCycle_Pending_Active_Blocked_Restored_Active()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);

        // Pending → Active
        (await _client.PostAsync($"/api/v1/user-accounts/{userId}/activate", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Active → Blocked
        (await _client.PostAsync($"/api/v1/user-accounts/{userId}/block?reason=E2E+full+cycle+test", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Blocked → Active (restore)
        (await _client.PostAsync($"/api/v1/user-accounts/{userId}/restore", null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify final state via GraphQL
        using var doc = await GqlUserAccountByIdAsync(userId, ct);
        doc.RootElement.GetProperty("data").GetProperty("userAccountById")
            .GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task ActivateUserAccount_NonExistent_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var res = await _client.PostAsync($"/api/v1/user-accounts/{Guid.NewGuid()}/activate", null, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BlockUserAccount_AlreadyBlocked_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);
        await _client.PostAsync($"/api/v1/user-accounts/{userId}/activate", null, ct);
        await _client.PostAsync($"/api/v1/user-accounts/{userId}/block?reason=First+block", null, ct);

        var second = await _client.PostAsync($"/api/v1/user-accounts/{userId}/block?reason=Second+block", null, ct);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE (GDPR soft-delete)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserAccount_ExistingAccount_Returns204AndNotAccessibleViaGql()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);

        var deleteRes = await _client.DeleteAsync($"/api/v1/user-accounts/{userId}", ct);
        deleteRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // After GDPR deletion the account should not be retrievable via GraphQL either
        using var doc = await GqlUserAccountByIdAsync(userId, ct);
        var user = doc.RootElement.GetProperty("data").GetProperty("userAccountById");
        user.ValueKind.Should().Be(JsonValueKind.Null,
            because: "soft-deleted accounts should not be returned by queries");
    }

    [Fact]
    public async Task DeleteUserAccount_NonExistent_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var res = await _client.DeleteAsync($"/api/v1/user-accounts/{Guid.NewGuid()}", ct);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AUTHENTICATION ATTEMPT (audit trail)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordAuthAttempt_Success_Returns204()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);
        var payload = new { userAccountId = userId, success = true, reason = "Valid credentials", ipAddress = "10.0.0.1" };

        var res = await _client.PostAsJsonAsync($"/api/v1/user-accounts/{userId}/authentication-attempts", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RecordAuthAttempt_Failure_Returns204()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);
        var payload = new { userAccountId = userId, success = false, reason = "Wrong password", ipAddress = "192.168.1.10" };

        var res = await _client.PostAsJsonAsync($"/api/v1/user-accounts/{userId}/authentication-attempts", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RecordAuthAttempt_NonExistentUser_Returns404()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var ghost = Guid.NewGuid();
        var payload = new { userAccountId = ghost, success = false, reason = "Invalid user", ipAddress = "127.0.0.1" };

        var res = await _client.PostAsJsonAsync($"/api/v1/user-accounts/{ghost}/authentication-attempts", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RecordAuthAttempt_MissingReason_Returns400()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker required.");
        var ct = TestContext.Current.CancellationToken;

        var userId = await CreateUserAndGetId(ct);
        var payload = new { userAccountId = userId, success = true, reason = "", ipAddress = "10.0.0.1" };

        var res = await _client.PostAsJsonAsync($"/api/v1/user-accounts/{userId}/authentication-attempts", payload, ct);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

    /// <summary>Queries userAccountById(userAccountId) via GraphQL. Caller must dispose.</summary>
    private Task<JsonDocument> GqlUserAccountByIdAsync(Guid userId, CancellationToken ct) =>
        GqlQueryAsync(
            $"{{ userAccountById(userAccountId: \"{userId}\") {{ userAccountId tenantId email category status }} }}",
            ct);

    private record UserPayload(Guid TenantId, Guid? BranchId, string Email, string Category, string? IdentityReference, string? IdentityReferenceType);
    private static UserPayload NewUserPayload(Guid tenantId)
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        return new UserPayload(tenantId, null, $"user.{uid}@e2e-test.local", "Internal", null, null);
    }

    private async Task<Guid> CreateTenantId(CancellationToken ct)
    {
        var uid = Guid.NewGuid().ToString("N")[..10].ToUpper();
        var payload = new { code = $"T{uid}", name = $"E2E UA Tenant {uid}", type = "CLIENT", idpStrategy = (string?)null, companyReference = (string?)null, isManagementOwner = true };
        var response = await _client.PostAsJsonAsync("/api/v1/tenants", payload, ct);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            System.IO.File.WriteAllText("e2e_tenant_error.txt", err);
        }
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        var idString = location!.Split('/').Last();
        var id = Guid.Parse(idString);

        _client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", id.ToString());

        return id;
    }

    private async Task<Guid> CreateUserAndGetId(CancellationToken ct)
    {
        var tenantId = await CreateTenantId(ct);
        var res = await _client.PostAsJsonAsync("/api/v1/user-accounts", NewUserPayload(tenantId), ct);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ReadGuid(res, "userAccountId", ct);
    }

    private static async Task<Guid> ReadGuid(HttpResponseMessage response, string property, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.GetProperty(property).GetGuid();
    }
}

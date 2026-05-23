using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

/// <summary>
/// REC-15: Integration tests that exercise the Tenant REST endpoints against a real
/// SQL Server instance running in a Testcontainer.
///
/// Prerequisites: Docker must be running locally.  Tests are automatically skipped
/// when Docker is unavailable so CI pipelines without Docker don't break.
/// </summary>
[Collection("SqlServer")]
public sealed class SqlServerTenantTests
{
    private readonly SqlServerContainerFixture _fixture;
    private readonly HttpClient _client;

    public SqlServerTenantTests(SqlServerContainerFixture fixture)
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

            // DevAuth middleware — provides actor identity without a real IdP
            _client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000001");
            _client.DefaultRequestHeaders.Add("X-User-Name", "integration-test");
        }
        else
        {
            // Provide a non-null client so the field is always assigned; tests will
            // skip before any HTTP call is issued via Skip.If() checks.
            _client = new HttpClient();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tenant creation & retrieval
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTenant_WithValidData_Returns201AndPersistsTenant()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker is required for SQL Server integration tests.");

        var body = new
        {
            code = $"TC-{Guid.NewGuid():N}"[..12],
            name = "Integration Test Corp",
            type = "Customer",
            idpStrategy = (string?)null,
            companyReference = (string?)null,
        };

        var response = await _client.PostAsJsonAsync("/api/v1/tenants", body, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        using var payload = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.GetProperty("tenantId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetTenants_AfterCreate_ReturnsCreatedTenant()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker is required for SQL Server integration tests.");

        // Arrange — create a uniquely identifiable tenant first
        var uniqueCode = $"QT-{Guid.NewGuid():N}"[..12];
        var createBody = new
        {
            code = uniqueCode,
            name = "Query Test Org",
            type = "Customer",
            idpStrategy = (string?)null,
            companyReference = (string?)null,
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/tenants", createBody, TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act — query the list
        var listResponse = await _client.GetAsync(
            $"/api/v1/tenants?page=1&pageSize=50&search={uniqueCode}&criteria=name",
            TestContext.Current.CancellationToken);

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(
            await listResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var items = payload.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0,
            because: "the newly created tenant should appear in the list");

        var found = Enumerable.Range(0, items.GetArrayLength())
            .Select(i => items[i])
            .Any(t => string.Equals(
                t.GetProperty("code").GetString(), uniqueCode,
                StringComparison.OrdinalIgnoreCase));

        found.Should().BeTrue(because: "the created tenant should be retrievable by code");
    }

    [Fact]
    public async Task CreateTenant_WithDuplicateCode_Returns409()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker is required for SQL Server integration tests.");

        var duplicateCode = $"DC-{Guid.NewGuid():N}"[..12];
        var body = new
        {
            code = duplicateCode,
            name = "First Org",
            type = "Customer",
            idpStrategy = (string?)null,
            companyReference = (string?)null,
        };

        // First creation should succeed
        var first = await _client.PostAsJsonAsync(
            "/api/v1/tenants", body, TestContext.Current.CancellationToken);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        // Second creation with same code should fail
        var second = await _client.PostAsJsonAsync(
            "/api/v1/tenants", body with { name = "Duplicate Org" },
            TestContext.Current.CancellationToken);
        second.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetTenants_Pagination_ReturnsCorrectPageMetadata()
    {
        if (!_fixture.IsAvailable) Assert.Skip("Docker is required for SQL Server integration tests.");

        var listResponse = await _client.GetAsync(
            "/api/v1/tenants?page=1&pageSize=5",
            TestContext.Current.CancellationToken);

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(
            await listResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        payload.RootElement.GetProperty("page").GetInt32().Should().Be(1);
        payload.RootElement.GetProperty("pageSize").GetInt32().Should().Be(5);
        payload.RootElement.GetProperty("totalItems").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        payload.RootElement.GetProperty("totalPages").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        payload.RootElement.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }
}

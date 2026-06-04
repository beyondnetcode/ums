using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Configuration;

/// <summary>
/// FS-20 REST endpoint integration tests.
/// Covers: full lifecycle (create → update → publish → archive),
/// scope-based authorization guards, error cases (404, 409, 422).
/// </summary>
public sealed class AppConfigurationRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _adminClient;
    private readonly HttpClient _tenantClient;

    // Seeded tenant used by UmsApiWebApplicationFactory
    private static readonly Guid SeededTenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private static readonly Guid SeededSystemSuiteId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public AppConfigurationRestEndpointTests(UmsApiWebApplicationFactory factory)
    {
        _adminClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        // No X-Tenant-Id → defaults to InternalAdminTenantId → IsInternalAdmin = true
        _adminClient.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000010");
        _adminClient.DefaultRequestHeaders.Add("X-User-Name", "FS20 Admin Tester");

        _tenantClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        _tenantClient.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000020");
        _tenantClient.DefaultRequestHeaders.Add("X-User-Name", "FS20 Tenant Tester");
        _tenantClient.DefaultRequestHeaders.Add("X-Tenant-Id", SeededTenantId.ToString());
        _tenantClient.DefaultRequestHeaders.Add("X-Is-Internal-Admin", "false");
    }

    // -----------------------------------------------------------------------
    // Full lifecycle
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateUpdatePublishArchive_GlobalConfig_ShouldSucceed()
    {
        var code = $"FS20-GLOBAL-{Guid.NewGuid():N}";

        // 1. Create (Draft)
        var createResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId      = (Guid?)null,
            systemSuiteId = (Guid?)null,
            moduleId      = (Guid?)null,
            code,
            value         = "3600",
            description   = "FS-20 global config lifecycle test",
            isInheritable = true,
            isEncrypted   = false,
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var configId = createPayload.RootElement.GetProperty("appConfigurationId").GetGuid();

        // 2. GET by ID — verify Draft + Global scope + initial version
        var getResponse = await _adminClient.GetAsync($"/api/v1/app-configurations/{configId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var getPayload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        getPayload.RootElement.GetProperty("status").GetString().Should().Be("Draft");
        getPayload.RootElement.GetProperty("scope").GetString().Should().Be("Global");
        getPayload.RootElement.GetProperty("version").GetString().Should().Be("1.0.0");

        // 3. Update (only Draft allows updates)
        var updateResponse = await _adminClient.PutAsJsonAsync($"/api/v1/app-configurations/{configId}", new
        {
            appConfigurationId = configId,
            value              = "7200",
            description        = "Updated description",
        }, TestContext.Current.CancellationToken);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 4. GET — verify updated value and minor version bump
        var getUpdatedResponse = await _adminClient.GetAsync($"/api/v1/app-configurations/{configId}", TestContext.Current.CancellationToken);
        using var updatedPayload = JsonDocument.Parse(await getUpdatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        updatedPayload.RootElement.GetProperty("value").GetString().Should().Be("7200");
        updatedPayload.RootElement.GetProperty("version").GetString().Should().Be("1.1.0");

        // 5. Publish
        var publishResponse = await _adminClient.PostAsync(
            $"/api/v1/app-configurations/{configId}/publish", null, TestContext.Current.CancellationToken);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. GET — verify Published
        var getPublishedResponse = await _adminClient.GetAsync($"/api/v1/app-configurations/{configId}", TestContext.Current.CancellationToken);
        using var publishedPayload = JsonDocument.Parse(await getPublishedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        publishedPayload.RootElement.GetProperty("status").GetString().Should().Be("Published");

        // 7. Archive
        var archiveResponse = await _adminClient.PostAsync(
            $"/api/v1/app-configurations/{configId}/archive", null, TestContext.Current.CancellationToken);
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 8. GET — verify Archived
        var getArchivedResponse = await _adminClient.GetAsync($"/api/v1/app-configurations/{configId}", TestContext.Current.CancellationToken);
        using var archivedPayload = JsonDocument.Parse(await getArchivedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        archivedPayload.RootElement.GetProperty("status").GetString().Should().Be("Archived");
    }

    [Fact]
    public async Task CreatePublish_TenantConfig_ShouldSucceed()
    {
        var code = $"FS20-TENANT-{Guid.NewGuid():N}";

        var createResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId      = SeededTenantId,
            systemSuiteId = (Guid?)null,
            moduleId      = (Guid?)null,
            code,
            value         = "60",
            description   = "FS-20 tenant-scoped config test",
            isInheritable = false,
            isEncrypted   = false,
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var configId = createPayload.RootElement.GetProperty("appConfigurationId").GetGuid();

        var getResponse = await _adminClient.GetAsync($"/api/v1/app-configurations/{configId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var getPayload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        getPayload.RootElement.GetProperty("scope").GetString().Should().Be("Tenant");
        getPayload.RootElement.GetProperty("status").GetString().Should().Be("Draft");

        var publishResponse = await _adminClient.PostAsync(
            $"/api/v1/app-configurations/{configId}/publish", null, TestContext.Current.CancellationToken);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getPublishedResponse = await _adminClient.GetAsync($"/api/v1/app-configurations/{configId}", TestContext.Current.CancellationToken);
        using var publishedPayload = JsonDocument.Parse(await getPublishedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        publishedPayload.RootElement.GetProperty("status").GetString().Should().Be("Published");
        publishedPayload.RootElement.GetProperty("tenantId").GetGuid().Should().Be(SeededTenantId);
    }

    // -----------------------------------------------------------------------
    // Query and filtering
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAppConfigurationById_WhenNotFound_ShouldReturn404()
    {
        var response = await _adminClient.GetAsync(
            $"/api/v1/app-configurations/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAppConfigurations_WithScopeGlobalFilter_ShouldReturnOnlyGlobalConfigs()
    {
        // Create at least one Global config so the assertion is meaningful
        var code = $"FS20-GFILTER-{Guid.NewGuid():N}";
        await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId = (Guid?)null, systemSuiteId = (Guid?)null, moduleId = (Guid?)null,
            code, value = "1", description = "Scope filter test", isInheritable = false, isEncrypted = false,
        }, TestContext.Current.CancellationToken);

        var response = await _adminClient.GetAsync(
            "/api/v1/app-configurations?page=1&pageSize=100&scope=Global", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var items = payload.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        Enumerable.Range(0, items.GetArrayLength())
            .Select(i => items[i].GetProperty("scope").GetString())
            .Should().OnlyContain(s => s == "Global");
    }

    [Fact]
    public async Task GetAppConfigurations_AsTenantAdmin_ShouldNotSeeGlobalScope()
    {
        var response = await _tenantClient.GetAsync(
            "/api/v1/app-configurations?page=1&pageSize=100&scope=Global", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------------
    // Authorization guards
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateAppConfiguration_WithGlobalScope_AsTenantAdmin_ShouldReturn403()
    {
        var response = await _tenantClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId      = (Guid?)null,
            systemSuiteId = (Guid?)null,
            moduleId      = (Guid?)null,
            code          = $"FS20-FORBID-{Guid.NewGuid():N}",
            value         = "forbidden",
            description   = "Tenant admin should not create global config",
            isInheritable = false,
            isEncrypted   = false,
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAppConfiguration_ForOtherTenant_AsTenantAdmin_ShouldReturn403()
    {
        var otherTenantId = Guid.NewGuid();

        var response = await _tenantClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId      = otherTenantId,
            systemSuiteId = (Guid?)null,
            moduleId      = (Guid?)null,
            code          = $"FS20-CROSS-{Guid.NewGuid():N}",
            value         = "forbidden",
            description   = "Cross-tenant creation attempt",
            isInheritable = false,
            isEncrypted   = false,
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishAppConfiguration_WithGlobalScope_AsTenantAdmin_ShouldReturn403()
    {
        // Internal admin creates a global config
        var code = $"FS20-PUBL-403-{Guid.NewGuid():N}";
        var createResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId = (Guid?)null, systemSuiteId = (Guid?)null, moduleId = (Guid?)null,
            code, value = "100", description = "Admin creates, tenant tries to publish",
            isInheritable = false, isEncrypted = false,
        }, TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var configId = createPayload.RootElement.GetProperty("appConfigurationId").GetGuid();

        // Tenant admin attempts to publish — should be 403
        var publishResponse = await _tenantClient.PostAsync(
            $"/api/v1/app-configurations/{configId}/publish", null, TestContext.Current.CancellationToken);

        publishResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------------
    // Error cases
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateAppConfiguration_WithDuplicateScopeAndCode_ShouldReturn409()
    {
        var code = $"FS20-DUP-{Guid.NewGuid():N}";

        var firstResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId = SeededTenantId, systemSuiteId = SeededSystemSuiteId, moduleId = (Guid?)null,
            code, value = "first", description = "First instance",
            isInheritable = false, isEncrypted = false,
        }, TestContext.Current.CancellationToken);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId = SeededTenantId, systemSuiteId = SeededSystemSuiteId, moduleId = (Guid?)null,
            code, // same code + same scope
            value = "second", description = "Duplicate instance",
            isInheritable = false, isEncrypted = false,
        }, TestContext.Current.CancellationToken);

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateAppConfiguration_WhenPublished_ShouldReturn422()
    {
        var code = $"FS20-UPD-422-{Guid.NewGuid():N}";

        var createResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId = (Guid?)null, systemSuiteId = (Guid?)null, moduleId = (Guid?)null,
            code, value = "original", description = "Will be published",
            isInheritable = false, isEncrypted = false,
        }, TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var configId = createPayload.RootElement.GetProperty("appConfigurationId").GetGuid();

        await _adminClient.PostAsync($"/api/v1/app-configurations/{configId}/publish", null, TestContext.Current.CancellationToken);

        var updateResponse = await _adminClient.PutAsJsonAsync($"/api/v1/app-configurations/{configId}", new
        {
            appConfigurationId = configId,
            value              = "changed",
            description        = "Cannot update published config",
        }, TestContext.Current.CancellationToken);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ArchiveAppConfiguration_WhenDraft_ShouldReturn422()
    {
        var code = $"FS20-ARC-422-{Guid.NewGuid():N}";

        var createResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId = (Guid?)null, systemSuiteId = (Guid?)null, moduleId = (Guid?)null,
            code, value = "v", description = "Archive from Draft should fail",
            isInheritable = false, isEncrypted = false,
        }, TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var configId = createPayload.RootElement.GetProperty("appConfigurationId").GetGuid();

        // Draft → Archive is forbidden: must go Draft → Published → Archived
        var archiveResponse = await _adminClient.PostAsync(
            $"/api/v1/app-configurations/{configId}/archive", null, TestContext.Current.CancellationToken);

        archiveResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PublishAppConfiguration_WhenAlreadyPublished_ShouldReturn422()
    {
        var code = $"FS20-PUBL-DUP-{Guid.NewGuid():N}";

        var createResponse = await _adminClient.PostAsJsonAsync("/api/v1/app-configurations", new
        {
            tenantId = (Guid?)null, systemSuiteId = (Guid?)null, moduleId = (Guid?)null,
            code, value = "v", description = "Double-publish should fail",
            isInheritable = false, isEncrypted = false,
        }, TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var configId = createPayload.RootElement.GetProperty("appConfigurationId").GetGuid();

        await _adminClient.PostAsync($"/api/v1/app-configurations/{configId}/publish", null, TestContext.Current.CancellationToken);

        var secondPublishResponse = await _adminClient.PostAsync(
            $"/api/v1/app-configurations/{configId}/publish", null, TestContext.Current.CancellationToken);

        secondPublishResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}

using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Configuration;

public sealed class ConfigurationRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ConfigurationRestEndpointTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        _client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000123");
        _client.DefaultRequestHeaders.Add("X-User-Name", "Integration Tester");
    }

    [Fact]
    public async Task GetFeatureFlags_ShouldReturnSeededFeatureFlag()
    {
        var response = await _client.GetAsync("/api/v1/feature-flags?page=1&pageSize=10", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
        payload.RootElement.GetProperty("items")[0].GetProperty("flagCode").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateFeatureFlag_ThenGetById_ShouldSucceed()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/feature-flags", new
        {
            flagCode = $"tenant_feature_{Guid.NewGuid():N}",
            flagType = "Boolean",
            flagTargets = "tenant-console",
            linkedResourceType = "Module",
            linkedResourceId = "33333333-3333-3333-3333-333333333333",
            rolloutPercentage = (int?)null,
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createdPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var featureFlagId = createdPayload.RootElement.GetProperty("featureFlagId").GetGuid();

        var getResponse = await _client.GetAsync($"/api/v1/feature-flags/{featureFlagId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var getPayload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        getPayload.RootElement.GetProperty("featureFlagId").GetGuid().Should().Be(featureFlagId);
    }

    [Fact]
    public async Task GetAppConfigurations_ShouldReturnSeededConfiguration()
    {
        var response = await _client.GetAsync("/api/v1/app-configurations?page=1&pageSize=10", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var items = payload.RootElement.GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        // Verify the well-known seeded config exists (order-independent)
        var codes = Enumerable.Range(0, items.GetArrayLength())
            .Select(i => items[i].GetProperty("code").GetString())
            .ToList();
        codes.Should().Contain("SESSION_TIMEOUT_MINUTES");
    }
}

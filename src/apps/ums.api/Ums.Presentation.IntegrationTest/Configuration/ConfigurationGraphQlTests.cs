using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Configuration;

public sealed class ConfigurationGraphQlTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ConfigurationGraphQlTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task GraphQlTenantsQuery_ShouldReturnSeededTenants()
    {
        var request = new
        {
            query = """
                query {
                  tenants(page: 1, pageSize: 10) {
                    items {
                      tenantId
                      code
                      name
                    }
                  }
                }
                """
        };

        var response = await _client.PostAsJsonAsync("/graphql", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        payload.RootElement.GetProperty("data").GetProperty("tenants").GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GraphQlFeatureFlagsQuery_ShouldReturnSeededFlag()
    {
        var request = new
        {
            query = """
                query {
                  featureFlags(page: 1, pageSize: 10) {
                    items {
                      featureFlagId
                      flagCode
                      status
                    }
                  }
                }
                """
        };

        var response = await _client.PostAsJsonAsync("/graphql", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        payload.RootElement.GetProperty("data").GetProperty("featureFlags").GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GraphQlIdpConfigurationsQuery_ShouldReturnSeededConfiguration()
    {
        var request = new
        {
            query = """
                query {
                  idpConfigurations(page: 1, pageSize: 10) {
                    items {
                      idpConfigurationId
                      providerType
                      status
                    }
                  }
                }
                """
        };

        var response = await _client.PostAsJsonAsync("/graphql", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        payload.RootElement.GetProperty("data").GetProperty("idpConfigurations").GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }
}

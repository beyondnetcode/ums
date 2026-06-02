using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Ums.Infrastructure.Persistence.Seeders;
using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Identity;

public sealed class AuthRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthRestEndpointTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task Login_WithSeededCommercialTenantCredentials_ShouldSucceed()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            tenantCode = "RANSA_PERU",
            username = "gerente.operaciones@ransa.pe",
            password = CoreDevDataSeeder.SuperAdminPassword,
            rememberMe = false,
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.GetProperty("tenantCode").GetString().Should().Be("RANSA_PERU");
        payload.RootElement.GetProperty("email").GetString().Should().Be("gerente.operaciones@ransa.pe");
        payload.RootElement.GetProperty("isInternalAdmin").GetBoolean().Should().BeFalse();
    }
}

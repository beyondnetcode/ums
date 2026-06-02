using System.Net;
using System.Text.Json;
using Ums.Infrastructure.Persistence.Seeders;
using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Identity;

public sealed class OnboardingInboxRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OnboardingInboxRestEndpointTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        _client.DefaultRequestHeaders.Add("X-User-Id", CoreDevDataSeeder.SuperAdminUserId);
        _client.DefaultRequestHeaders.Add("X-User-Name", "Integration Tester");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", CoreDevDataSeeder.InternalAdminTenantId);
    }

    [Fact]
    public async Task UserSignupsInbox_ShouldExposeInternalAdminSeededPendingUser()
    {
        var response = await _client.GetAsync("/api/v1/onboarding/inbox/user-signups", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var items = payload.RootElement.EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        items.Should().Contain(item =>
            string.Equals(item.GetProperty("email").GetString(), "bandeja.admin@ums.local", StringComparison.OrdinalIgnoreCase) &&
            item.GetProperty("tenantId").GetGuid() == Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId));
    }

    [Fact]
    public async Task ProfileRequestsInbox_ShouldExposeInternalAdminSeededPendingRequest()
    {
        var response = await _client.GetAsync("/api/v1/onboarding/inbox/profile-requests", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var items = payload.RootElement.EnumerateArray().ToList();

        items.Should().NotBeEmpty();
        items.Should().Contain(item =>
            item.GetProperty("justification").GetString()?.Contains("administrador interno", StringComparison.OrdinalIgnoreCase) == true);
    }
}

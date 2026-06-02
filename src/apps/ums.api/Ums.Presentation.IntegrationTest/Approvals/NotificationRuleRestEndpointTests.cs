using Ums.Presentation.IntegrationTest.Infrastructure;
using Ums.Infrastructure.Persistence.Seeders;

namespace Ums.Presentation.IntegrationTest.Approvals;

public sealed class NotificationRuleRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NotificationRuleRestEndpointTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        _client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000123");
        _client.DefaultRequestHeaders.Add("X-User-Name", "Integration Tester");
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", CoreDevDataSeeder.InternalAdminTenantId);
    }

    [Fact]
    public async Task CreateNotificationRule_WithEmailRecipient_ShouldNormalizeRecipient()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/notification-rules", new
        {
            tenantId = Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId),
            channel = "Email",
            recipient = "  Alerts@BeyondNet.Com "
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var notificationRuleId = createPayload.RootElement.GetProperty("notificationRuleId").GetGuid();

        var getResponse = await _client.GetAsync($"/api/v1/notification-rules/{notificationRuleId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.GetProperty("recipient").GetString().Should().Be("alerts@beyondnet.com");
        payload.RootElement.GetProperty("channel").GetString().Should().Be("Email");
    }

    [Fact]
    public async Task CreateNotificationRule_WithInvalidSmsRecipient_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/notification-rules", new
        {
            tenantId = Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId),
            channel = "Sms",
            recipient = "invalid-recipient"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

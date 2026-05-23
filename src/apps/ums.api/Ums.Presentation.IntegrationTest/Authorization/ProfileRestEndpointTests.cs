using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Authorization;

public sealed class ProfileRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProfileRestEndpointTests(UmsApiWebApplicationFactory factory)
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
    public async Task CreateProfile_Deactivate_Activate_ShouldSucceed()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/profiles", new
        {
            tenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            userId = Guid.NewGuid(),
            roleId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            branchId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createdPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var profileId = createdPayload.RootElement.GetProperty("profileId").GetGuid();

        var getCreatedResponse = await _client.GetAsync($"/api/v1/profiles/{profileId}", TestContext.Current.CancellationToken);
        getCreatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var createdProfilePayload = JsonDocument.Parse(await getCreatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        createdProfilePayload.RootElement.GetProperty("profileId").GetGuid().Should().Be(profileId);
        createdProfilePayload.RootElement.GetProperty("scope").GetString().Should().Be("BranchScoped");
        createdProfilePayload.RootElement.GetProperty("isActive").GetBoolean().Should().BeTrue();

        var deactivateResponse = await _client.PostAsync($"/api/v1/profiles/{profileId}/deactivate", null, TestContext.Current.CancellationToken);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getDeactivatedResponse = await _client.GetAsync($"/api/v1/profiles/{profileId}", TestContext.Current.CancellationToken);
        using var deactivatedPayload = JsonDocument.Parse(await getDeactivatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        deactivatedPayload.RootElement.GetProperty("isActive").GetBoolean().Should().BeFalse();

        var activateResponse = await _client.PostAsync($"/api/v1/profiles/{profileId}/activate", null, TestContext.Current.CancellationToken);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getActivatedResponse = await _client.GetAsync($"/api/v1/profiles/{profileId}", TestContext.Current.CancellationToken);
        using var activatedPayload = JsonDocument.Parse(await getActivatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        activatedPayload.RootElement.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetProfileById_WhenMissing_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/profiles/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

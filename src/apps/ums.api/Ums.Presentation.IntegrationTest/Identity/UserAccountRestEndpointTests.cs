using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Identity;

public sealed class UserAccountRestEndpointTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserAccountRestEndpointTests(UmsApiWebApplicationFactory factory)
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
    public async Task CreateUserAccount_Activate_Block_Restore_ShouldSucceed()
    {
        var email = $"integration.user.{Guid.NewGuid():N}@ums.local";
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user-accounts", new
        {
            tenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            branchId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            email,
            category = "Internal",
            identityReference = "EMP-001",
            identityReferenceType = "HrId",
        }, TestContext.Current.CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createdPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var userAccountId = createdPayload.RootElement.GetProperty("userAccountId").GetGuid();

        var getCreatedResponse = await _client.GetAsync($"/api/v1/user-accounts/{userAccountId}", TestContext.Current.CancellationToken);
        getCreatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var createdUserPayload = JsonDocument.Parse(await getCreatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        createdUserPayload.RootElement.GetProperty("userAccountId").GetGuid().Should().Be(userAccountId);
        createdUserPayload.RootElement.GetProperty("status").GetString().Should().Be("Pending");
        createdUserPayload.RootElement.GetProperty("branchId").GetGuid().Should().Be(Guid.Parse("55555555-5555-5555-5555-555555555555"));

        var activateResponse = await _client.PostAsync($"/api/v1/user-accounts/{userAccountId}/activate", null, TestContext.Current.CancellationToken);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getActivatedResponse = await _client.GetAsync($"/api/v1/user-accounts/{userAccountId}", TestContext.Current.CancellationToken);
        using var activatedPayload = JsonDocument.Parse(await getActivatedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        activatedPayload.RootElement.GetProperty("status").GetString().Should().Be("Active");

        var blockResponse = await _client.PostAsync($"/api/v1/user-accounts/{userAccountId}/block?reason=manual-review", null, TestContext.Current.CancellationToken);
        blockResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getBlockedResponse = await _client.GetAsync($"/api/v1/user-accounts/{userAccountId}", TestContext.Current.CancellationToken);
        using var blockedPayload = JsonDocument.Parse(await getBlockedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        blockedPayload.RootElement.GetProperty("status").GetString().Should().Be("Blocked");

        var restoreResponse = await _client.PostAsync($"/api/v1/user-accounts/{userAccountId}/restore", null, TestContext.Current.CancellationToken);
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getRestoredResponse = await _client.GetAsync($"/api/v1/user-accounts/{userAccountId}", TestContext.Current.CancellationToken);
        using var restoredPayload = JsonDocument.Parse(await getRestoredResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        restoredPayload.RootElement.GetProperty("status").GetString().Should().Be("Active");

        var duplicateCreateResponse = await _client.PostAsJsonAsync("/api/v1/user-accounts", new
        {
            tenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            branchId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            email,
            category = "Internal",
            identityReference = "EMP-002",
            identityReferenceType = "HrId",
        }, TestContext.Current.CancellationToken);

        duplicateCreateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetUserAccountById_WhenMissing_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/user-accounts/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetLocalPassword_ForNativeUser_ShouldExposeStatusWithoutHash()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user-accounts", new
        {
            tenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            email = $"native.password.{Guid.NewGuid():N}@ums.local",
            category = "Internal",
        }, TestContext.Current.CancellationToken);

        using var createdPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var userAccountId = createdPayload.RootElement.GetProperty("userAccountId").GetGuid();

        var setPasswordResponse = await _client.PostAsJsonAsync($"/api/v1/user-accounts/{userAccountId}/passwords", new
        {
            userAccountId,
            password = "Temporary!Pass123",
        }, TestContext.Current.CancellationToken);

        setPasswordResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await _client.GetAsync($"/api/v1/user-accounts/{userAccountId}", TestContext.Current.CancellationToken);
        using var accountPayload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        accountPayload.RootElement.GetProperty("hasActivePassword").GetBoolean().Should().BeTrue();
        accountPayload.RootElement.GetProperty("passwordUpdatedAtUtc").ValueKind.Should().NotBe(JsonValueKind.Null);
        accountPayload.RootElement.TryGetProperty("passwordHash", out _).Should().BeFalse();
    }
}

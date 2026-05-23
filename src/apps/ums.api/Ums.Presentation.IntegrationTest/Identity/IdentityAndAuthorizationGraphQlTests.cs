using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Identity;

public sealed class IdentityAndAuthorizationGraphQlTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IdentityAndAuthorizationGraphQlTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
    }

    [Fact]
    public async Task GraphQlUserAccountsQuery_ShouldReturnItems()
    {
        var request = new
        {
            query = """
                query {
                  userAccounts(page: 1, pageSize: 10) {
                    items {
                      userAccountId
                      email
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
        payload.RootElement.GetProperty("data").GetProperty("userAccounts").GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GraphQlProfilesQuery_ShouldReturnPayload()
    {
        var request = new
        {
            query = """
                query {
                  profiles(page: 1, pageSize: 10) {
                    items {
                      profileId
                      scope
                      isActive
                    }
                  }
                }
                """
        };

        var response = await _client.PostAsJsonAsync("/graphql", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        payload.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
        payload.RootElement.GetProperty("data").GetProperty("profiles").GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
    }
}

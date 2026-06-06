using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PactNet;
using PactNet.Matchers;
using Ums.ContractTest.Infrastructure;

namespace Ums.ContractTest.Consumers;

/// <summary>
/// OPS-02: Consumer contract tests for the Auth API.
/// </summary>
public sealed class AuthConsumerTests : IDisposable
{
    private readonly IPactBuilderV4 _pactBuilder;

    private static readonly string PactsDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "pacts");

    public AuthConsumerTests(ITestOutputHelper output)
    {
        var config = new PactConfig
        {
            PactDir    = PactsDir,
            Outputters = [new XunitOutput(output)],
            LogLevel   = PactLogLevel.Warn,
        };

        _pactBuilder = Pact.V4("ums-web-app", "ums-api", config).WithHttpInteractions();
    }

    [Fact]
    [Trait("pact", "consumer")]
    public async Task PostToken_WithValidCredentials_Returns200()
    {
        _pactBuilder
            .UponReceiving("a valid token request")
            .Given("a user account exists with valid credentials")
            .WithRequest(HttpMethod.Post, "/api/v1/auth/token")
            .WithHeader("Content-Type", Match.Regex("application/json.*", "application/json; charset=utf-8"))
            .WithJsonBody(new
            {
                email    = Match.Type("user@example.com"),
                password = Match.Type("ValidPassword123!"),
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", Match.Regex("application/json.*", "application/json; charset=utf-8"))
            .WithJsonBody(new
            {
                token = Match.Type("eyJhbGciOiJIUzI1NiIsInR5cCI..."),
                user  = new
                {
                    userId = Match.Type("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    email  = Match.Type("user@example.com"),
                }
            });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            using var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            using var content = JsonContent.Create(new
            {
                email    = "user@example.com",
                password = "ValidPassword123!",
            });

            var response = await client.PostAsync("/api/v1/auth/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("token", out _), "response should have 'token'");
        });
    }

    [Fact]
    [Trait("pact", "consumer")]
    public async Task PostToken_WithInvalidCredentials_Returns401()
    {
        _pactBuilder
            .UponReceiving("an invalid token request")
            .Given("a user account does not exist or credentials do not match")
            .WithRequest(HttpMethod.Post, "/api/v1/auth/token")
            .WithHeader("Content-Type", Match.Regex("application/json.*", "application/json; charset=utf-8"))
            .WithJsonBody(new
            {
                email    = Match.Type("wrong@example.com"),
                password = Match.Type("WrongPassword123!"),
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.Unauthorized)
            .WithHeader("Content-Type", Match.Type("application/problem+json"))
            .WithJsonBody(new
            {
                status = Match.Type(401),
                title  = Match.Type("Unauthorized"),
            });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            using var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            using var content = JsonContent.Create(new
            {
                email    = "wrong@example.com",
                password = "WrongPassword123!",
            });

            var response = await client.PostAsync("/api/v1/auth/token", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        });
    }

    public void Dispose() { }
}

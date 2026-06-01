using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Ums.Sdk.Authorization;
using Ums.Sdk.Authorization.Testing;
using Ums.Sdk.Client;
using Ums.Sdk.Contracts;
using Xunit;

namespace Ums.Sdk.Tests;

public sealed class UmsAuthClientTests
{
    private static UmsAuthClient CreateClient(HttpMessageHandler handler, UmsSdkClientOptions? options = null)
    {
        options ??= new UmsSdkClientOptions { BaseAddress = new Uri("https://ums.example.com") };
        var http = new HttpClient(handler) { BaseAddress = options.BaseAddress };
        return new UmsAuthClient(http, Options.Create(options));
    }

    [Fact]
    public async Task AuthenticateAsync_HappyPath_ReturnsTypedResult()
    {
        var graph = AuthGraphBuilder.ForTenant("LOGISTICS_CORE")
            .WithUser("ana.flores@example.com")
            .WithScope("PURCHASE_ORDER.VIEW")
            .Build();

        var payload = new
        {
            token = "TOKEN.PLACEHOLDER.SIG",
            tokenType = "Bearer",
            expiresIn = 3600,
            issuedAt = DateTimeOffset.UtcNow,
            format = "JSON",
            graph,
            requestId = Guid.NewGuid()
        };

        var handler = new StubHandler(HttpStatusCode.OK, JsonSerializer.Serialize(payload));
        var client = CreateClient(handler);

        var result = await client.AuthenticateAsync(new ClientAuthRequest("LOGISTICS_CORE", "ana.flores", "p"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("TOKEN.PLACEHOLDER.SIG");
        result.Value.Graph.SchemaVersion.Should().Be(SchemaVersion.Current);
        result.Value.Graph.Context.Tenant.Code.Should().Be("LOGISTICS_CORE");
    }

    [Fact]
    public async Task AuthenticateAsync_UnsupportedMajor_ReturnsAuth205()
    {
        var graph = AuthGraphBuilder.ForTenant("LOGISTICS_CORE")
            .WithSchemaVersion("2.0.0")
            .Build();

        var payload = new
        {
            token = "T",
            tokenType = "Bearer",
            expiresIn = 60,
            issuedAt = DateTimeOffset.UtcNow,
            format = "JSON",
            graph,
            requestId = Guid.NewGuid()
        };

        var handler = new StubHandler(HttpStatusCode.OK, JsonSerializer.Serialize(payload));
        var client = CreateClient(handler);

        var result = await client.AuthenticateAsync(new ClientAuthRequest("LOGISTICS_CORE", "u", "p"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(UmsErrorCodes.AuthGraphSchemaUnsupported);
    }

    [Fact]
    public async Task AuthenticateAsync_ServerReturns401_ReturnsInvalidCredentials()
    {
        var handler = new StubHandler(HttpStatusCode.Unauthorized, "{}");
        var client = CreateClient(handler);

        var result = await client.AuthenticateAsync(new ClientAuthRequest("LOGISTICS_CORE", "u", "bad"));

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(UmsErrorCodes.InvalidCredentials);
    }

    [Fact]
    public async Task AuthenticateAsync_ServerReturns404_ReturnsTenantNotFound()
    {
        var handler = new StubHandler(HttpStatusCode.NotFound, "tenant not found");
        var client = CreateClient(handler);

        var result = await client.AuthenticateAsync(new ClientAuthRequest("ZZZ", "u", "p"));

        result.ErrorCode.Should().Be(UmsErrorCodes.TenantNotFound);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public StubHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}

using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ums.Sdk.Authorization;
using Ums.Sdk.Authorization.AspNetCore;
using Ums.Sdk.Authorization.Testing;
using Ums.Sdk.Contracts;
using Xunit;

namespace Ums.Sdk.Tests;

public sealed class UmsAuthGraphMiddlewareTests
{
    [Fact]
    public async Task Middleware_Decodes_Graph_From_Bearer_And_Stores_It_On_HttpContext()
    {
        var graph = AuthGraphBuilder.ForTenant("LOGISTICS_CORE").WithScope("PURCHASE_ORDER.VIEW").Build();
        var jwt = BuildFakeJwt(graph);
        AuthorizationGraph? observed = null;

        using var host = await BuildHost(observe => observed = observe);
        using var client = host.GetTestServer().CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("/probe");
        response.IsSuccessStatusCode.Should().BeTrue();

        observed.Should().NotBeNull();
        observed!.Context.Tenant.Code.Should().Be("LOGISTICS_CORE");
        observed.SchemaVersion.Should().Be(SchemaVersion.Current);
    }

    [Fact]
    public async Task Middleware_Without_Bearer_Leaves_Context_Empty()
    {
        AuthorizationGraph? observed = null;
        using var host = await BuildHost(observe => observed = observe);
        using var client = host.GetTestServer().CreateClient();

        var response = await client.GetAsync("/probe");
        response.IsSuccessStatusCode.Should().BeTrue();
        observed.Should().BeNull();
    }

    [Fact]
    public async Task Middleware_RejectsExpiredGraphs_With_401_AUTH_201()
    {
        var graph = AuthGraphBuilder.ForTenant("LOGISTICS_CORE").BuildExpired();
        var jwt = BuildFakeJwt(graph);

        using var host = await BuildHost(_ => { }, opt => opt.RejectExpiredGraphs = true);
        using var client = host.GetTestServer().CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("/probe");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"AUTH_201\"");
    }

    [Fact]
    public async Task Middleware_RejectsIncompatibleSchemaVersion_With_401_AUTH_205()
    {
        var graph = AuthGraphBuilder.ForTenant("LOGISTICS_CORE")
            .WithSchemaVersion("2.0.0")
            .Build();
        var jwt = BuildFakeJwt(graph);

        using var host = await BuildHost(_ => { });
        using var client = host.GetTestServer().CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        var response = await client.GetAsync("/probe");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"AUTH_205\"");
    }

    private static async Task<IHost> BuildHost(
        Action<AuthorizationGraph?> observe,
        Action<UmsAuthGraphMiddlewareOptions>? configure = null)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddUmsAuthGraphMiddleware();
                    if (configure is not null) services.Configure(configure);
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseUmsAuthGraph();
                    app.UseRouting();
                    app.UseEndpoints(e => e.MapGet("/probe", ctx =>
                    {
                        var stored = ctx.Items.TryGetValue("UmsAuthGraph", out var raw)
                            ? raw as AuthorizationGraph
                            : null;
                        observe(stored);
                        ctx.Response.StatusCode = 200;
                        return Task.CompletedTask;
                    }));
                }));

        var host = await builder.StartAsync();
        return host;
    }

    private static string BuildFakeJwt(AuthorizationGraph graph)
    {
        // Header.Payload.Signature — JWT-style three-segment compact serialization. Signature is
        // irrelevant for the middleware since it does NOT validate the signature here (verification
        // is expected to happen in an upstream authentication step).
        var header = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"));
        var payloadJson = JsonSerializer.Serialize(new { graph });
        var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        return $"{header}.{payload}.";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

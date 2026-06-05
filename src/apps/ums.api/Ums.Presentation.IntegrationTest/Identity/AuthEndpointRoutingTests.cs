using Microsoft.AspNetCore.Routing;
using FluentAssertions;
using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Identity;

public sealed class AuthEndpointRoutingTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly EndpointDataSource _endpointDataSource;

    public AuthEndpointRoutingTests(UmsApiWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        _endpointDataSource = scope.ServiceProvider.GetRequiredService<EndpointDataSource>();
    }

    [Fact]
    public void VisualLoginAndClientAuthentication_ShouldBeMappedToDifferentApiRoots()
    {
        var routes = _endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .ToArray();

        routes.Should().Contain("/api/v1/auth/login");
        routes.Should().Contain("/api/v1/client/authenticate");
        routes.Should().NotContain("/api/v1/auth/client/authenticate");
    }
}

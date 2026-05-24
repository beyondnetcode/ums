using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ums.Globalization;
using Ums.Presentation.Bootstrapping;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

public sealed class UmsApiServiceBootstrappersTests
{
    [Fact]
    public void AddUmsApiServiceBootstrappers_ShouldRegisterCoreServices()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddUmsApiServiceBootstrappers(configuration, new FakeHostEnvironment());

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IMediator));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(ILocalizationService));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IRequestContext));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(HealthCheckService));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(ICorsService));
    }

    [Fact]
    public void AddUmsApiServiceBootstrappers_ShouldRegisterPlatformPolicies()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddUmsApiServiceBootstrappers(configuration, new FakeHostEnvironment());

        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IConfigureOptions<RateLimiterOptions>));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IConfigureOptions<CorsOptions>));
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedOrigins"] = "https://localhost:3000",
                ["Persistence:Provider"] = "InMemory",
            })
            .Build();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "Ums.Presentation.IntegrationTest";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

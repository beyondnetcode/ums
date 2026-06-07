using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

/// <summary>
/// Base class for integration tests providing a configured HttpClient and fixture.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly PostgreSqlContainerFixture Fixture;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(PostgreSqlContainerFixture fixture)
    {
        Fixture = fixture;
        if (fixture.IsAvailable)
        {
            var factory = new PostgreSqlWebApplicationFactory(fixture.ConnectionString);
            Client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
            });
            Client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000001");
            Client.DefaultRequestHeaders.Add("X-User-Name", "integration-test");
        }
        else
        {
            Client = new HttpClient();
        }
    }

    public void Dispose()
    {
        Client?.Dispose();
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

[Collection("PostgreSql")]
public sealed class PostgreSqlAuthorizationPersistenceTests : IntegrationTestBase
{
    private static readonly Guid TenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    public PostgreSqlAuthorizationPersistenceTests(PostgreSqlContainerFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CreateAndGetSystemSuite_UsesPostgreSqlAuthorizationStore()
    {
        if (!Fixture.IsAvailable) Assert.Skip("Docker is required for SQL Server integration tests.");

        var code = $"SS{Guid.NewGuid():N}"[..10];
        var createBody = new
        {
            tenantId = TenantId,
            code,
            name = "Tenant Console",
            description = "SQL-backed authorization system suite."
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/system-suites", createBody, TestContext.Current.CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var systemSuiteId = createPayload.RootElement.GetProperty("systemSuiteId").GetGuid();

        var getResponse = await Client.GetAsync($"/api/v1/system-suites/{systemSuiteId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var getPayload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        getPayload.RootElement.GetProperty("systemSuiteId").GetGuid().Should().Be(systemSuiteId);
        getPayload.RootElement.GetProperty("tenantId").GetGuid().Should().Be(TenantId);
        getPayload.RootElement.GetProperty("code").GetString().Should().Be(code);
    }

    [Fact]
    public async Task CreatePublishAndGetPermissionTemplate_UsesPostgreSqlAuthorizationStore()
    {
        if (!Fixture.IsAvailable) Assert.Skip("Docker is required for SQL Server integration tests.");

        var suiteCode = $"PT{Guid.NewGuid():N}"[..10];
        var createSuite = await Client.PostAsJsonAsync("/api/v1/system-suites", new
        {
            tenantId = TenantId,
            code = suiteCode,
            name = "Approvals",
            description = "System suite for template integration."
        }, TestContext.Current.CancellationToken);
        createSuite.StatusCode.Should().Be(HttpStatusCode.Created);

        using var suitePayload = JsonDocument.Parse(await createSuite.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var systemSuiteId = suitePayload.RootElement.GetProperty("systemSuiteId").GetGuid();
        var roleId = Guid.NewGuid();

        var createTemplate = await Client.PostAsJsonAsync("/api/v1/permission-templates", new
        {
            tenantId = TenantId,
            roleId,
            systemSuiteId
        }, TestContext.Current.CancellationToken);
        createTemplate.StatusCode.Should().Be(HttpStatusCode.Created);

        using var templatePayload = JsonDocument.Parse(await createTemplate.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var templateId = templatePayload.RootElement.GetProperty("templateId").GetGuid();

        var publishResponse = await Client.PostAsync($"/api/v1/permission-templates/{templateId}/publish", content: null, TestContext.Current.CancellationToken);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/v1/permission-templates/{templateId}", TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var getPayload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        getPayload.RootElement.GetProperty("templateId").GetGuid().Should().Be(templateId);
        getPayload.RootElement.GetProperty("tenantId").GetGuid().Should().Be(TenantId);
        getPayload.RootElement.GetProperty("roleId").GetGuid().Should().Be(roleId);
        getPayload.RootElement.GetProperty("systemSuiteId").GetGuid().Should().Be(systemSuiteId);
        getPayload.RootElement.GetProperty("status").GetString().Should().Be("Published");
    }
}

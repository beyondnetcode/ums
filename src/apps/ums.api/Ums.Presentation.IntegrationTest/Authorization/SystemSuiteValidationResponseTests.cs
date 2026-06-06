using Ums.Presentation.IntegrationTest.Infrastructure;

namespace Ums.Presentation.IntegrationTest.Authorization;

public sealed class SystemSuiteValidationResponseTests : IClassFixture<UmsApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SystemSuiteValidationResponseTests(UmsApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });
        _client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000123");
        _client.DefaultRequestHeaders.Add("X-User-Name", "Integration Tester");
        _client.DefaultRequestHeaders.Add("X-Language", "es");
    }

    [Fact]
    public async Task AddModule_DescriptionTooLong_ReturnsActionableMessageWithoutTechnicalDetails()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/system-suites/{Guid.NewGuid()}/modules",
            new
            {
                code = "SECURITY",
                name = "Security",
                description = new string('x', 501),
                sortOrder = 1,
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var problem = document.RootElement;

        problem.GetProperty("detail").GetString()
            .Should().NotBeNullOrWhiteSpace("validation message should describe the error");
        var errorId = problem.GetProperty("errorId").GetString();
        Guid.TryParse(errorId, out _).Should().BeTrue();
        problem.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
        problem.GetRawText().Should().NotContain("stackTrace");
    }

    [Fact]
    public async Task AddModule_InvalidCode_ReturnsFieldRuleValueAndCorrectionHint()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/system-suites/{Guid.NewGuid()}/modules",
            new
            {
                code = "DDDD-!",
                name = "Security",
                description = "Safe description",
                sortOrder = 1,
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        document.RootElement.GetProperty("detail").GetString()
            .Should().NotBeNullOrWhiteSpace("validation message should describe the error");
        Guid.TryParse(document.RootElement.GetProperty("errorId").GetString(), out _).Should().BeTrue();
    }
}

using Microsoft.AspNetCore.Mvc.Testing;
using PactNet.Verifier;
using Ums.ContractTest.Infrastructure;

namespace Ums.ContractTest.Provider;

/// <summary>
/// OPS-02: Provider verification tests for ums-api.
///
/// Starts the real API in InMemory mode (via ContractTestWebApplicationFactory) and
/// replays each pact file recorded by consumer tests, verifying that the real API
/// honours every interaction.
///
/// State handlers supply the preconditions that consumer interactions declare via
/// the "Given(…)" clause.  Each handler uses the InMemory stores so no database is
/// needed.
///
/// Run order (enforced by CI):
///   1. Consumer tests  → writes pacts/*.json
///   2. Provider tests  → reads pacts/*.json and verifies against live API
/// </summary>
[Collection("Provider")]
public sealed class UmsApiProviderTests : IClassFixture<ContractTestWebApplicationFactory>
{
    private readonly ContractTestWebApplicationFactory _factory;
    private readonly ITestOutputHelper                 _output;

    // Must match the directory where consumer tests write pacts.
    private static readonly string PactsDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "pacts");

    public UmsApiProviderTests(ContractTestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output  = output;
    }

    [Fact]
    [Trait("pact", "provider")]
    public void VerifyAllPacts_ForUmsWebApp()
    {
        // Spin up the real API on a random port inside WebApplicationFactory.
        using var server = _factory.Server;
        var baseUri     = server.BaseAddress;

        var config = new PactVerifierConfig
        {
            Outputters = [new PactNet.Output.Xunit.XunitOutput(_output)],
            LogLevel   = PactLogLevel.Warn,
        };

        // Load every pact where the consumer is "ums-web-app".
        var pactFiles = Directory.EnumerateFiles(PactsDir, "ums-web-app-ums-api.json");
        if (!pactFiles.Any())
        {
            _output.WriteLine("No pact files found. Run consumer tests first.");
            return;
        }

        new PactVerifier("ums-api", config)
            .WithHttpEndpoint(baseUri)
            .WithFileSource(new DirectoryInfo(PactsDir))
            .WithProviderStateUrl(new Uri(baseUri, "/_pact/provider-states"))
            .Verify();
    }
}

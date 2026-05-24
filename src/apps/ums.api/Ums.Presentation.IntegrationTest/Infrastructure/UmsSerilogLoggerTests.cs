using Microsoft.Extensions.Logging;
using Ums.Infrastructure.Aop;
using Ums.Infrastructure.Services;
using Ums.Shell.Aop;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

using MelILogger = Microsoft.Extensions.Logging.ILogger;
using MelILoggerProvider = Microsoft.Extensions.Logging.ILoggerProvider;

public sealed class UmsSerilogLoggerTests
{
    [Fact]
    public void OnEntry_ShouldEmitFullObservabilityEnvelope()
    {
        var provider = new CapturingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(provider);
        });

        var executionContext = new RequestContextAccessor();
        executionContext.Set(new ExecutionContextSnapshot(
            CorrelationId: "corr-001",
            SessionTrackingId: "session-001",
            TraceId: "trace-001",
            SpanId: "span-001"));

        var logger = new UmsSerilogLogger(loggerFactory, new StubUserContext("tenant-001"), executionContext);

        logger.OnEntry(
            CreateJoinPoint(),
            [new Argument { Name = "request", Type = "CreateFeatureFlagCommand", Value = new object() }],
            requestId: "fallback-request-id");

        provider.Entries.Should().ContainSingle();
        var entry = provider.Entries.Single();

        entry.Level.Should().Be(LogLevel.Information);
        entry.Properties["TenantId"].Should().Be("tenant-001");
        entry.Properties["CorrelationId"].Should().Be("corr-001");
        entry.Properties["SessionTrackingId"].Should().Be("session-001");
        entry.Properties["TraceId"].Should().Be("trace-001");
        entry.Properties["SpanId"].Should().Be("span-001");
        entry.Properties["BoundedContext"].Should().Be("Configuration");
    }

    [Fact]
    public void OnExit_ShouldKeepSessionAndTraceFields()
    {
        var provider = new CapturingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddProvider(provider);
        });

        var executionContext = new RequestContextAccessor();
        executionContext.Set(new ExecutionContextSnapshot(
            CorrelationId: "corr-002",
            SessionTrackingId: "session-002",
            TraceId: "trace-002",
            SpanId: "span-002"));

        var logger = new UmsSerilogLogger(loggerFactory, new StubUserContext("tenant-002"), executionContext);

        logger.OnExit(CreateJoinPoint(), requestId: string.Empty, duration: 42L);

        provider.Entries.Should().ContainSingle();
        var entry = provider.Entries.Single();

        entry.Properties["TenantId"].Should().Be("tenant-002");
        entry.Properties["CorrelationId"].Should().Be("corr-002");
        entry.Properties["SessionTrackingId"].Should().Be("session-002");
        entry.Properties["TraceId"].Should().Be("trace-002");
        entry.Properties["SpanId"].Should().Be("span-002");
    }

    private static IJoinPoint CreateJoinPoint()
        => new JoinPoint
        {
            TargetObject = new Ums.Application.Configuration.FeatureFlag.Commands.FakeFeatureFlagHandler(),
            TargetType = typeof(Ums.Application.Configuration.FeatureFlag.Commands.FakeFeatureFlagHandler),
            MethodInfo = typeof(Ums.Application.Configuration.FeatureFlag.Commands.FakeFeatureFlagHandler)
                .GetMethod(nameof(Ums.Application.Configuration.FeatureFlag.Commands.FakeFeatureFlagHandler.Handle))!,
            Arguments = [],
        };

    private sealed record CapturedLogEntry(
        LogLevel Level,
        string Message,
        Dictionary<string, object?> Properties,
        Exception? Exception);

    private sealed class CapturingLoggerProvider : MelILoggerProvider
    {
        public List<CapturedLogEntry> Entries { get; } = [];

        public MelILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(List<CapturedLogEntry> entries) : MelILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state is IEnumerable<KeyValuePair<string, object?>> keyValuePairs
                ? keyValuePairs.ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, object?>();

            entries.Add(new CapturedLogEntry(logLevel, formatter(state, exception), properties, exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }

    private sealed class StubUserContext(string? tenantId) : IUserContext
    {
        public string? UserId => "test-user";

        public string? UserName => "Test User";

        public string? TenantId => tenantId;

        public bool IsAuthenticated => true;
    }
}

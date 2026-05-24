using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Diagnostics;
using Ums.Shell.Aop.Aspects;
using Ums.Shell.Aop.Aspects.Logger.Serilog;

namespace Ums.Shell.Aop.Tests
{
    [TestClass]
    public class ObservabilityExecutionContextTests
    {
        [TestMethod]
        public void ResolveExecutionContext_ShouldPreferAccessorSnapshot()
        {
            var accessor = new TestExecutionContextAccessor
            {
                Current = new ExecutionContextSnapshot("corr-a", "session-a", "trace-a", "span-a")
            };

            var sut = new TestStructuredLogger(accessor);

            var snapshot = sut.Resolve("request-fallback");

            snapshot.CorrelationId.ShouldBe("corr-a");
            snapshot.SessionTrackingId.ShouldBe("session-a");
            snapshot.TraceId.ShouldBe("trace-a");
            snapshot.SpanId.ShouldBe("span-a");
        }

        [TestMethod]
        public void ResolveExecutionContext_ShouldFallbackToActivityBaggageAndRequestId()
        {
            using var activity = new Activity("test");
            activity.Start();
            activity.SetBaggage(ObservabilityKeys.CorrelationId, "corr-b");
            activity.SetBaggage(ObservabilityKeys.SessionTrackingId, "session-b");

            var accessor = new TestExecutionContextAccessor
            {
                Current = ExecutionContextSnapshot.Empty
            };

            var sut = new TestStructuredLogger(accessor);

            var snapshot = sut.Resolve("request-fallback");

            snapshot.CorrelationId.ShouldBe("corr-b");
            snapshot.SessionTrackingId.ShouldBe("session-b");
            snapshot.TraceId.ShouldNotBeNullOrWhiteSpace();
            snapshot.SpanId.ShouldNotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void ResolveExecutionContext_ShouldFallbackToRequestIdWhenNoBaggageExists()
        {
            var accessor = new TestExecutionContextAccessor
            {
                Current = ExecutionContextSnapshot.Empty
            };

            var sut = new TestStructuredLogger(accessor);

            var snapshot = sut.Resolve("request-c");

            snapshot.CorrelationId.ShouldBe("request-c");
            snapshot.SessionTrackingId.ShouldBeEmpty();
        }

        [TestMethod]
        public void InferBoundedContext_ShouldReturnNamespaceSegment()
        {
            var boundedContext = TestStructuredLogger.BoundedContextOf(typeof(Ums.Application.Identity.Tenant.Queries.FakeHandler));

            boundedContext.ShouldBe("Identity");
        }

        private sealed class TestExecutionContextAccessor : IExecutionContextAccessor
        {
            public ExecutionContextSnapshot Current { get; set; } = ExecutionContextSnapshot.Empty;

            public void Set(ExecutionContextSnapshot snapshot)
            {
                Current = snapshot;
            }
        }

        private sealed class TestStructuredLogger : StructuredAopLoggerBase
        {
            public TestStructuredLogger(IExecutionContextAccessor executionContextAccessor)
                : base(executionContextAccessor)
            {
            }

            public ExecutionContextSnapshot Resolve(string requestId) => ResolveExecutionContext(requestId);

            public static string BoundedContextOf(System.Type targetType) => InferBoundedContext(targetType);

            public override void OnExit(IJoinPoint joinPoint, Return @return, string requestId, long duration)
            {
            }

            public override void OnExit(IJoinPoint joinPoint, string requestId, long duration)
            {
            }

            public override void OnExit(IJoinPoint joinPoint, Return @return, string requestId)
            {
            }

            public override void OnExit(IJoinPoint joinPoint, string requestId)
            {
            }

            public override void OnEntry(IJoinPoint joinPoint, Argument[] arguments, string requestId)
            {
            }

            public override void OnException(IJoinPoint joinPoint, string requestId, System.Exception ex)
            {
            }
        }
    }
}

namespace Ums.Application.Identity.Tenant.Queries
{
    internal sealed class FakeHandler;
}

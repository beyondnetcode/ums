using Ums.Shell.Aop.Aspects;
using Ums.Shell.Aop.Aspects.Logger.Serilog;
using Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Shouldly;
using System;

namespace Ums.Shell.Aop.Tests.Microsoft.Extensions.DependencyInjection
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Proxy_WithOneAspect_ShoudBe()
        {
            AssertWithProvider(
                configure => configure.AddAspect<Add>(),
                provider => new TestCases().Proxy_WithOneAspect_ShoudBe(provider));
        }

        [TestMethod]
        public void Proxy_WithMultipleAspect_ShoudBe()
        {
            AssertWithProvider(
                configure =>
                {
                    configure.AddAspect<Add10>();
                    configure.AddAspect<Multiple5>();
                    configure.AddAspect<Subtract20>();
                },
                provider => new TestCases().Proxy_WithMultipleAspect_ShoudBe(provider));
        }

        [TestMethod]
        public void Proxy_WithAdviceAspect_ShoudBe()
        {
            AssertWithProvider(
                configure => configure.AddAdvice<AddAdvice>(),
                provider => new TestCases().Proxy_WithAdviceAspect_ShoudBe(provider));
        }

        [TestMethod]
        public void Proxy_WithLogAspect_ShoudBe()
        {
            AssertWithLogger(provider => new TestCases().Proxy_WithLogAspect_ShoudBe(provider));
        }

        [TestMethod]
        public void Proxy_WithLogAspectAndExpression_ShoudBe()
        {
            AssertWithLogger(provider => new TestCases().Proxy_WithLogAspectAndExpression_ShoudBe(provider));
        }

        [TestMethod]
        public void Proxy_WithLogAspectAndComplexExpression_ShoudBe()
        {
            AssertWithLogger(provider => new TestCases().Proxy_WithLogAspectAndComplexExpression_ShoudBe(provider));
        }

        [TestMethod]
        public void Proxy_UsesScopedLifetime()
        {
            var services = ConfigureServices(configure => configure.AddAspect<Add>());

            using var root = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
            using var firstScope = root.CreateScope();
            using var secondScope = root.CreateScope();

            var first = firstScope.ServiceProvider.GetRequiredService<INumberProvider>();
            var repeated = firstScope.ServiceProvider.GetRequiredService<INumberProvider>();
            var second = secondScope.ServiceProvider.GetRequiredService<INumberProvider>();

            first.ShouldBeSameAs(repeated);
            first.ShouldNotBeSameAs(second);
        }

        [TestMethod]
        public void Proxy_DoesNotAllowSingletonLifetime()
        {
            var services = new ServiceCollection();

            Should.Throw<ArgumentException>(() =>
                services.AddAopProxy<INumberProvider, NumberProvider>(ServiceLifetime.Singleton));
        }

        [TestMethod]
        public void Proxy_AllowsTransientLifetime()
        {
            var services = new ServiceCollection();

            services.AddAop(configure => configure.AddAspect<Add>());
            services.AddAopProxy<INumberProvider, NumberProvider>(ServiceLifetime.Transient);

            using var root = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            var first = root.GetRequiredService<INumberProvider>();
            var second = root.GetRequiredService<INumberProvider>();

            first.ShouldNotBeSameAs(second);
        }

        private static void AssertWithLogger(Action<INumberProvider> assertion)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Verbose()
                .CreateLogger();

            AssertWithProvider(configure => configure.AddLogger<SerilogLogger>(), assertion);
        }

        private static void AssertWithProvider(
            Action<IAopAspectsBuilder> configure,
            Action<INumberProvider> assertion)
        {
            var services = ConfigureServices(configure);

            using var root = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
            using var scope = root.CreateScope();

            assertion(scope.ServiceProvider.GetRequiredService<INumberProvider>());
        }

        private static IServiceCollection ConfigureServices(Action<IAopAspectsBuilder> configure)
        {
            var services = new ServiceCollection();

            services.AddAop(configure);
            services.AddAopProxy<INumberProvider, NumberProvider>();

            return services;
        }
    }
}

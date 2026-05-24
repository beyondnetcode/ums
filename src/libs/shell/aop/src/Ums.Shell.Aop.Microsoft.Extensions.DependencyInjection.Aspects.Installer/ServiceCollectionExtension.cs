using Ums.Shell.Aop.Aspects;
using Ums.Shell.Aop.DispatchProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAop(
            this IServiceCollection services,
            Action<IAopAspectsBuilder> configure = null)
        {
            var builder = new AopAspectsBuilder(services, new List<Type>());

            builder.AddAspect<LoggerAspect>();
            builder.AddAspect<AdviceAspect>();
            builder.AddAspect<RetryAspect>();
            builder.AddAdvice<Advice>();

            configure?.Invoke(builder);

            services.AddSingleton<IPointCut, PointCut>();
            services.AddTransient<IAspectExecutor>(provider => new AspectExecutor(
                builder.Types.ToArray(),
                type => provider.GetRequiredKeyedService<IAspect>(type),
                provider.GetRequiredService<IPointCut>()));
            services.AddTransient<IFactory<IAdvice>>(provider => new Factory<IAdvice>(
                type => provider.GetRequiredKeyedService<IAdvice>(type)));
            services.AddTransient<IFactory<ILogger>>(provider => new Factory<ILogger>(
                type => provider.GetRequiredKeyedService<ILogger>(type)));
            services.AddSingleton<IEvaluator, Evaluator>();

            return services;
        }

        public static IServiceCollection AddAopProxy<TService, TImplementation>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TService : class
            where TImplementation : class, TService
        {
            if (lifetime == ServiceLifetime.Singleton)
            {
                throw new ArgumentException(
                    "Singleton AOP proxies are not supported because aspects may depend on scoped services.",
                    nameof(lifetime));
            }

            services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));
            services.Add(new ServiceDescriptor(
                typeof(TService),
                provider => AopProxyCreator.Create<TService, TImplementation>(
                    provider.GetRequiredService<TImplementation>(),
                    provider.GetRequiredService<IAspectExecutor>()),
                lifetime));

            return services;
        }
    }
}

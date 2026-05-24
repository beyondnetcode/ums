using Ums.Shell.Aop.Aspects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer
{
    public class AopAspectsBuilder : IAopAspectsBuilder
    {
        private readonly IServiceCollection _container;

        public List<Type> Types { get; }

        public AopAspectsBuilder(IServiceCollection container, List<Type> types)
        {
            _container = container;

            Types = types;
        }

        public IAopAspectsBuilder AddAdvice<TImplementation>() where TImplementation : class, IAdvice
        {
            _container.AddKeyedTransient<IAdvice, TImplementation>(typeof(TImplementation));

            return this;
        }

        public IAopAspectsBuilder AddLogger<TImplementation>() where TImplementation : class, ILogger
        {
            _container.AddKeyedTransient<ILogger, TImplementation>(typeof(TImplementation));

            return this;
        }

        public IAopAspectsBuilder AddAspect<TImplementation>() where TImplementation : class, IAspect
        {
            var type = typeof(TImplementation);

            Types.Add(type);

            _container.AddKeyedTransient(typeof(IAspect), type, type);

            return this;
        }
    }
}

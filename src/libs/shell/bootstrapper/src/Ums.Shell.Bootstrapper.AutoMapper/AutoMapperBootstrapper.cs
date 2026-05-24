using System;
using AutoMapper;
using Ums.Shell.Bootstrapper.Interface;

namespace Ums.Shell.Bootstrapper.AutoMapper
{
    public class AutoMapperBootstrapper : IBootstrapper<MapperConfigurationExpression>
    {
        private readonly Action<IMapperConfigurationExpression>? _action;

        public AutoMapperBootstrapper(Action<IMapperConfigurationExpression>? action = null)
        {
            _action = action;
        }

        public void Run()
        {
            var expression = new MapperConfigurationExpression();
            _action?.Invoke(expression);
            Result = expression;
        }

        public MapperConfigurationExpression? Result { get; private set; }
    }
}
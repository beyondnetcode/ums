namespace Ums.Shell.Factory.Model
{
    public class SetupItem
    {
        public SetupItem(Type target, Type? implementation, Type service, string name, object? selector)
        {
            Name = name;

            Selector = selector;

            ImplementationType = implementation;

            TargetType = target;

            ServiceType = service;
        }

        public SetupItem(Type target, Type? implementation, Type service, string name)
        : this(target, implementation, service, name, null)
            {

            }

        public SetupItem(Type target, Type service, string name)
            : this(target, null, service, name)
        {

        }

        public string Name { get; }

        public Type TargetType { get; }

        public Type? ImplementationType { get; set; }

        public Type ServiceType { get; }

        public object? Selector { get; set; }
    }
}
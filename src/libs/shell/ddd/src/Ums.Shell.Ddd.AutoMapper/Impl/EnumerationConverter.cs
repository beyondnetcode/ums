using AutoMapper;

namespace Ums.Shell.Ddd.AutoMapper.Impl
{
    public class EnumerationConverter<TEnum> : ITypeConverter<int, TEnum> where TEnum : DomainEnumeration
    {
        public TEnum Convert(int source, TEnum destination, ResolutionContext context)
        {
            return DomainEnumeration.FromValue<TEnum>(source)!;
        }
    }
}

namespace Ums.Shell.Ddd.Test.AutoMapper
{
    public class ParentRootProfile : Profile
    {
        public ParentRootProfile()
        {
            CreateMap<AuditDto, AuditValueObject>().ConvertUsing<ParentRootAuditDtoToEntityConvert>();
            CreateMap<AuditValueObject, AuditDto>().ConvertUsing<ParentRootEntityAuditToAuditDtoConvert>();
            CreateMap<int, SampleEntityStatus>().ConvertUsing<EnumerationConverter<SampleEntityStatus>>();

            CreateMap<SampleEntityProps, SampleEntityDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.GetValue()))
                .ForMember(dest => dest.SampleReferenceId, opt => opt.MapFrom(src => src.SampleReferenceId.GetValue()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Id));

            CreateMap<SampleEntityDto, SampleEntityProps>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}

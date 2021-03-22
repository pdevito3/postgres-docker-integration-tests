namespace Accessioning.WebApi.Features.Samples.Mappings
{
    using Accessioning.Core.Dtos.Sample;
    using AutoMapper;
    using Accessioning.Core.Entities;

    public class SampleProfile : Profile
    {
        public SampleProfile()
        {
            //createmap<to this, from this>
            CreateMap<Sample, SampleDto>()
                .ReverseMap();
            CreateMap<SampleForCreationDto, Sample>();
            CreateMap<SampleForUpdateDto, Sample>()
                .ReverseMap();
        }
    }
}
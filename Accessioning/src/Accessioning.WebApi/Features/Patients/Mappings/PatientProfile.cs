namespace Accessioning.WebApi.Features.Patients.Mappings
{
    using Accessioning.Core.Dtos.Patient;
    using AutoMapper;
    using Accessioning.Core.Entities;

    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            //createmap<to this, from this>
            CreateMap<Patient, PatientDto>()
                .ReverseMap();
            CreateMap<PatientForCreationDto, Patient>();
            CreateMap<PatientForUpdateDto, Patient>()
                .ReverseMap();
        }
    }
}
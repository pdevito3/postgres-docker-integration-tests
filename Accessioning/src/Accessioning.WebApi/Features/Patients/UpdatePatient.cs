namespace Accessioning.WebApi.Features.Patients
{
    using Accessioning.Core.Entities;
    using Accessioning.Core.Dtos.Patient;
    using Accessioning.Core.Exceptions;
    using Accessioning.Infrastructure.Contexts;
    using Accessioning.WebApi.Features.Patients.Validators;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class UpdatePatient
    {
        public class UpdatePatientCommand : IRequest<bool>
        {
            public Guid PatientId { get; set; }
            public PatientForUpdateDto PatientToUpdate { get; set; }

            public UpdatePatientCommand(Guid patient, PatientForUpdateDto patientToUpdate)
            {
                PatientId = patient;
                PatientToUpdate = patientToUpdate;
            }
        }

        public class CustomUpdatePatientValidation : PatientForManipulationDtoValidator<PatientForUpdateDto>
        {
            public CustomUpdatePatientValidation()
            {
            }
        }

        public class Handler : IRequestHandler<UpdatePatientCommand, bool>
        {
            private readonly AccessioningDbContext _db;
            private readonly IMapper _mapper;

            public Handler(AccessioningDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<bool> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
            {
                // add logger (and a try catch with logger so i can cap the unexpected info)........ unless this happens in my logger decorator that i am going to add?

                var recordToUpdate = await _db.Patients
                    .FirstOrDefaultAsync(p => p.PatientId == request.PatientId);

                if (recordToUpdate == null)
                {
                    // log error
                    throw new KeyNotFoundException();
                }

                _mapper.Map(request.PatientToUpdate, recordToUpdate);
                var saveSuccessful = await _db.SaveChangesAsync() > 0;

                if (!saveSuccessful)
                {
                    // add log
                    throw new Exception("Unable to save the requested changes. Please check the logs for more information.");
                }

                return true;
            }
        }
    }
}
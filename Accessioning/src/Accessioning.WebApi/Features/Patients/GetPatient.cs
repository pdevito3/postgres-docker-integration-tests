namespace Accessioning.WebApi.Features.Patients
{
    using Accessioning.Core.Dtos.Patient;
    using Accessioning.Core.Exceptions;
    using Accessioning.Infrastructure.Contexts;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class GetPatient
    {
        public class PatientQuery : IRequest<PatientDto>
        {
            public Guid PatientId { get; set; }

            public PatientQuery(Guid patientId)
            {
                PatientId = patientId;
            }
        }

        public class Handler : IRequestHandler<PatientQuery, PatientDto>
        {
            private readonly AccessioningDbContext _db;
            private readonly IMapper _mapper;

            public Handler(AccessioningDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<PatientDto> Handle(PatientQuery request, CancellationToken cancellationToken)
            {
                // add logger (and a try catch with logger so i can cap the unexpected info)........ unless this happens in my logger decorator that i am going to add?

                // include marker -- to accomodate adding includes with craftsman commands, the next line must stay as `var result = await _db.Patients`. -- do not delete this comment
                var result = await _db.Patients
                    .ProjectTo<PatientDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(p => p.PatientId == request.PatientId);

                if (result == null)
                {
                    // log error
                    throw new KeyNotFoundException();
                }

                return result;
            }
        }
    }
}
namespace Accessioning.WebApi.Features.Patients
{
    using Accessioning.Core.Entities;
    using Accessioning.Core.Dtos.Patient;
    using Accessioning.Core.Exceptions;
    using Accessioning.Infrastructure.Contexts;
    using Accessioning.Core.Wrappers;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using MediatR;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetPatientList
    {
        public class PatientListQuery : IRequest<PagedList<PatientDto>>
        {
            public PatientParametersDto QueryParameters { get; set; }

            public PatientListQuery(PatientParametersDto queryParameters)
            {
                QueryParameters = queryParameters;
            }
        }

        public class Handler : IRequestHandler<PatientListQuery, PagedList<PatientDto>>
        {
            private readonly AccessioningDbContext _db;
            private readonly SieveProcessor _sieveProcessor;
            private readonly IMapper _mapper;

            public Handler(AccessioningDbContext db, IMapper mapper, SieveProcessor sieveProcessor)
            {
                _mapper = mapper;
                _db = db;
                _sieveProcessor = sieveProcessor;
            }

            public async Task<PagedList<PatientDto>> Handle(PatientListQuery request, CancellationToken cancellationToken)
            {
                if (request.QueryParameters == null)
                {
                    // log error
                    throw new ApiException("Invalid query parameters.");
                }

                // include marker -- to accomodate adding includes with craftsman commands, the next line must stay as `var result = await _db.Patients`. -- do not delete this comment
                var collection = _db.Patients
                    as IQueryable<Patient>;

                var sieveModel = new SieveModel
                {
                    Sorts = request.QueryParameters.SortOrder ?? "PatientId",
                    Filters = request.QueryParameters.Filters
                };

                collection = _sieveProcessor.Apply(sieveModel, collection);
                var dtoCollection = _db.Patients
                    .ProjectTo<PatientDto>(_mapper.ConfigurationProvider);

                return await PagedList<PatientDto>.CreateAsync(dtoCollection,
                    request.QueryParameters.PageNumber,
                    request.QueryParameters.PageSize);
            }
        }
    }
}
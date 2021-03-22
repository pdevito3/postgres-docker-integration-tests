namespace Accessioning.WebApi.Features.Samples
{
    using Accessioning.Core.Entities;
    using Accessioning.Core.Dtos.Sample;
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

    public class GetSampleList
    {
        public class SampleListQuery : IRequest<PagedList<SampleDto>>
        {
            public SampleParametersDto QueryParameters { get; set; }

            public SampleListQuery(SampleParametersDto queryParameters)
            {
                QueryParameters = queryParameters;
            }
        }

        public class Handler : IRequestHandler<SampleListQuery, PagedList<SampleDto>>
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

            public async Task<PagedList<SampleDto>> Handle(SampleListQuery request, CancellationToken cancellationToken)
            {
                if (request.QueryParameters == null)
                {
                    // log error
                    throw new ApiException("Invalid query parameters.");
                }

                // include marker -- to accomodate adding includes with craftsman commands, the next line must stay as `var result = await _db.Samples`. -- do not delete this comment
                var collection = _db.Samples
                    as IQueryable<Sample>;

                var sieveModel = new SieveModel
                {
                    Sorts = request.QueryParameters.SortOrder ?? "SampleId",
                    Filters = request.QueryParameters.Filters
                };

                collection = _sieveProcessor.Apply(sieveModel, collection);
                var dtoCollection = _db.Samples
                    .ProjectTo<SampleDto>(_mapper.ConfigurationProvider);

                return await PagedList<SampleDto>.CreateAsync(dtoCollection,
                    request.QueryParameters.PageNumber,
                    request.QueryParameters.PageSize);
            }
        }
    }
}
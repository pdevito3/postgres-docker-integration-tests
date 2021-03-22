namespace Accessioning.WebApi.Features.Samples
{
    using Accessioning.Core.Dtos.Sample;
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

    public class GetSample
    {
        public class SampleQuery : IRequest<SampleDto>
        {
            public Guid SampleId { get; set; }

            public SampleQuery(Guid sampleId)
            {
                SampleId = sampleId;
            }
        }

        public class Handler : IRequestHandler<SampleQuery, SampleDto>
        {
            private readonly AccessioningDbContext _db;
            private readonly IMapper _mapper;

            public Handler(AccessioningDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<SampleDto> Handle(SampleQuery request, CancellationToken cancellationToken)
            {
                // add logger (and a try catch with logger so i can cap the unexpected info)........ unless this happens in my logger decorator that i am going to add?

                // include marker -- to accomodate adding includes with craftsman commands, the next line must stay as `var result = await _db.Samples`. -- do not delete this comment
                var result = await _db.Samples
                    .ProjectTo<SampleDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(s => s.SampleId == request.SampleId);

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
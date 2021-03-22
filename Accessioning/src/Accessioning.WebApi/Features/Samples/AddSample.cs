namespace Accessioning.WebApi.Features.Samples
{
    using Accessioning.Core.Entities;
    using Accessioning.Core.Dtos.Sample;
    using Accessioning.Core.Exceptions;
    using Accessioning.Infrastructure.Contexts;
    using Accessioning.WebApi.Features.Samples.Validators;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class AddSample
    {
        public class AddSampleCommand : IRequest<SampleDto>
        {
            public SampleForCreationDto SampleToAdd { get; set; }

            public AddSampleCommand(SampleForCreationDto sampleToAdd)
            {
                SampleToAdd = sampleToAdd;
            }
        }

        public class CustomCreateSampleValidation : SampleForManipulationDtoValidator<SampleForCreationDto>
        {
            public CustomCreateSampleValidation()
            {
            }
        }

        public class Handler : IRequestHandler<AddSampleCommand, SampleDto>
        {
            private readonly AccessioningDbContext _db;
            private readonly IMapper _mapper;

            public Handler(AccessioningDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<SampleDto> Handle(AddSampleCommand request, CancellationToken cancellationToken)
            {
                var sample = _mapper.Map<Sample> (request.SampleToAdd);
                _db.Samples.Add(sample);
                var saveSuccessful = await _db.SaveChangesAsync() > 0;

                if (saveSuccessful)
                {
                    // include marker -- to accomodate adding includes with craftsman commands, the next line must stay as `var result = await _db.Samples`. -- do not delete this comment
                    return await _db.Samples
                        .ProjectTo<SampleDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync(s => s.SampleId == sample.SampleId);
                }
                else
                {
                    // add log
                    throw new Exception("Unable to save the new record. Please check the logs for more information.");
                }
            }
        }
    }
}
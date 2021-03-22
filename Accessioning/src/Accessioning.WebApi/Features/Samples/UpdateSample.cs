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

    public class UpdateSample
    {
        public class UpdateSampleCommand : IRequest<bool>
        {
            public Guid SampleId { get; set; }
            public SampleForUpdateDto SampleToUpdate { get; set; }

            public UpdateSampleCommand(Guid sample, SampleForUpdateDto sampleToUpdate)
            {
                SampleId = sample;
                SampleToUpdate = sampleToUpdate;
            }
        }

        public class CustomUpdateSampleValidation : SampleForManipulationDtoValidator<SampleForUpdateDto>
        {
            public CustomUpdateSampleValidation()
            {
            }
        }

        public class Handler : IRequestHandler<UpdateSampleCommand, bool>
        {
            private readonly AccessioningDbContext _db;
            private readonly IMapper _mapper;

            public Handler(AccessioningDbContext db, IMapper mapper)
            {
                _mapper = mapper;
                _db = db;
            }

            public async Task<bool> Handle(UpdateSampleCommand request, CancellationToken cancellationToken)
            {
                // add logger (and a try catch with logger so i can cap the unexpected info)........ unless this happens in my logger decorator that i am going to add?

                var recordToUpdate = await _db.Samples
                    .FirstOrDefaultAsync(s => s.SampleId == request.SampleId);

                if (recordToUpdate == null)
                {
                    // log error
                    throw new KeyNotFoundException();
                }

                _mapper.Map(request.SampleToUpdate, recordToUpdate);
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
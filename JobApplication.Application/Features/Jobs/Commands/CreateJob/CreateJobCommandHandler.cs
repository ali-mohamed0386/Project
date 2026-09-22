using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CreateJob
{
    public sealed class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, JobResponseDto>
    {
        private readonly IJobRepository _jobRepository;

        public CreateJobCommandHandler(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<JobResponseDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            var job = new Job
            {
                Title = request.Job.Title,
                Description = request.Job.Description,
                IsActive = true,
                // The RecruiterId comes from the token — not from the request.
                RecruiterId = request.RecruiterId,
            };

            await _jobRepository.InsertAsync(job);
            await _jobRepository.SaveChangesAsync();

            return Map(job);
        }

        private static JobResponseDto Map(Job job) => new()
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            IsActive = job.IsActive,
            RecruiterId = job.RecruiterId,
            ClosedAt = job.ClosedAt,
            ClosedBy = job.ClosedBy,
        };
    }
}

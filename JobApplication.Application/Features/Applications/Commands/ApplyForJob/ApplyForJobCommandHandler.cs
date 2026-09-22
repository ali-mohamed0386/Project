using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.ApplyForJob
{
    public sealed class ApplyForJobCommandHandler : IRequestHandler<ApplyForJobCommand, ApplicationResponseDto>
    {
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICandidateRepository _candidateRepository;

        public ApplyForJobCommandHandler(
            IJobApplicationRepository applicationRepository,
            IJobRepository jobRepository,
            ICandidateRepository candidateRepository)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _candidateRepository = candidateRepository;
        }

        public async Task<ApplicationResponseDto> Handle(
            ApplyForJobCommand request,
            CancellationToken cancellationToken)
        {
            // Rule 1: the job must exist
            var job = await _jobRepository.GetByIdAsync(request.Application.JobId)
                ?? throw new NotFoundException($"Job with id {request.Application.JobId} was not found.");

            // Rule 2: the candidate must exist
            // (only needed for the check — the data itself is not used here)
            if (await _candidateRepository.GetByIdAsync(request.CandidateId) is null)
                throw new NotFoundException($"Candidate with id {request.CandidateId} was not found.");

            // Rule 3: the job must be open
            if (!job.IsActive)
                throw new BadRequestException("This job is closed and no longer accepts applications.");

            // Rule 4: a candidate cannot apply twice for the same job
            if (await _applicationRepository.ExistsAsync(request.CandidateId, job.Id))
                throw new ConflictException("You have already applied for this job.");

            var now = DateTime.UtcNow;

            var application = new JobCandidateApplication
            {
                CandidateId = request.CandidateId,
                JobId = job.Id,
                // Rule 5: the initial status
                JobApplicationStatus = JobApplicationStatus.Applied,
                // Rule 6: this is the CreatedAt timestamp required by the spec,
                // stored in the column that already exists.
                AppliedAt = now,
                StatusUpdatedAt = now,
            };

            await _applicationRepository.InsertAsync(application);
            await _applicationRepository.SaveChangesAsync();

            return Map(application);
        }

        private static ApplicationResponseDto Map(JobCandidateApplication application) => new()
        {
            Id = application.Id,
            JobId = application.JobId,
            CandidateId = application.CandidateId,
            Status = application.JobApplicationStatus,
            AppliedAt = application.AppliedAt,
            StatusUpdatedAt = application.StatusUpdatedAt,
            CancelledAt = application.CancelledAt,
        };
    }
}

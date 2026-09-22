using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    /// <summary>
    /// Moves a job to the closed state, recording when and by whom it was closed.
    /// </summary>
    public sealed class CloseJobCommandHandler : IRequestHandler<CloseJobCommand>
    {
        private readonly IJobRepository _jobRepository;

        public CloseJobCommandHandler(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task Handle(CloseJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.GetByIdAsync(request.JobId)
                ?? throw new NotFoundException($"Job with id {request.JobId} was not found.");

            // Ownership rule: a recruiter may only close their own job.
            if (job.RecruiterId != request.RecruiterId)
                throw new ForbiddenException("You are not allowed to close a job you do not own.");

            // State rule: closing an already-closed job is rejected.
            if (!job.IsActive)
                throw new BadRequestException("This job is already closed.");

            var now = DateTime.UtcNow;

            job.IsActive = false;
            job.ClosedAt = now;
            // Record who closed it — the audit trail.
            job.ClosedBy = request.RecruiterId;

            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();
        }
    }
}

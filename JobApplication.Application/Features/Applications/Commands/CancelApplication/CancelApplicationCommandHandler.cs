using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.CancelApplication
{
    public sealed class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand>
    {
        private readonly IJobApplicationRepository _applicationRepository;

        public CancelApplicationCommandHandler(IJobApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetByIdWithJobAsync(request.ApplicationId)
                ?? throw new NotFoundException($"Application with id {request.ApplicationId} was not found.");

            // Only the applicant may cancel their own application
            if (application.CandidateId != request.CandidateId)
                throw new ForbiddenException("You are not allowed to cancel an application you do not own.");

            // Cancellation is only allowed in the Applied and UnderReview states
            if (!application.JobApplicationStatus.CanBeCancelled())
                throw new BadRequestException(
                    $"The application cannot be cancelled while it is in the " +
                    $"'{application.JobApplicationStatus}' state. " +
                    "Cancellation is only allowed in the Applied and UnderReview states.");

            var now = DateTime.UtcNow;

            // NOTE: this is a soft delete, not a physical delete.
            // Removing the row would let the candidate apply again for the same
            // job, bypassing the "no duplicate application" rule.
            application.JobApplicationStatus = JobApplicationStatus.Cancelled;
            application.CancelledAt = now;
            application.StatusUpdatedAt = now;

            _applicationRepository.Update(application);
            await _applicationRepository.SaveChangesAsync();
        }
    }
}

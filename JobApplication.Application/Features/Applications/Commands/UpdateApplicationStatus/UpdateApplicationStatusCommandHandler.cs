using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.UpdateApplicationStatus
{
    public sealed class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand>
    {
        private readonly IJobApplicationRepository _applicationRepository;

        public UpdateApplicationStatusCommandHandler(IJobApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
        {
            var application = await _applicationRepository.GetByIdWithJobAsync(request.ApplicationId)
                ?? throw new NotFoundException($"Application with id {request.ApplicationId} was not found.");

            // The recruiter must own the job the application was submitted for
            if (application.Job.RecruiterId != request.RecruiterId)
                throw new ForbiddenException("You are not allowed to update an application for a job you do not own.");

            // Forward-only: the permitted transitions are defined in the Domain
            if (!application.JobApplicationStatus.CanTransitionTo(request.Status.Status))
                throw new BadRequestException(
                    $"Cannot change the status from '{application.JobApplicationStatus}' to '{request.Status.Status}'. " +
                    "Only forward transitions are allowed.");

            application.JobApplicationStatus = request.Status.Status;
            application.StatusUpdatedAt = DateTime.UtcNow;

            _applicationRepository.Update(application);
            await _applicationRepository.SaveChangesAsync();
        }
    }
}

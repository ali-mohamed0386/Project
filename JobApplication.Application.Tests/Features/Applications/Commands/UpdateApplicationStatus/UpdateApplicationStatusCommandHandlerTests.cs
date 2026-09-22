using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Applications.Commands.UpdateApplicationStatus;
using JobApplication.Application.Tests.Fakes;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Xunit;

namespace JobApplication.Application.Tests.Features.Applications.Commands.UpdateApplicationStatus
{
    public class UpdateApplicationStatusCommandHandlerTests
    {
        private const int RecruiterId = 7;
        private const int OtherRecruiterId = 99;
        private const int ApplicationId = 3;

        private static JobCandidateApplication Application(
            JobApplicationStatus status = JobApplicationStatus.Applied,
            int recruiterId = RecruiterId) => new()
        {
            Id = ApplicationId,
            CandidateId = 5,
            JobId = 1,
            JobApplicationStatus = status,
            AppliedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            StatusUpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Job = new Job
            {
                Id = 1,
                Title = "Backend Developer",
                Description = "Build and maintain the public API.",
                IsActive = true,
                RecruiterId = recruiterId,
            },
        };

        private static UpdateApplicationStatusCommand Command(JobApplicationStatus target, int recruiterId = RecruiterId)
            => new(ApplicationId, new UpdateApplicationStatusDto { Status = target }, recruiterId);

        [Fact]
        public async Task Handle_WhenTransitionIsForward_MovesToTheNewStatus()
        {
            var application = Application();
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            await handler.Handle(Command(JobApplicationStatus.UnderReview), CancellationToken.None);

            Assert.Equal(JobApplicationStatus.UnderReview, application.JobApplicationStatus);
            Assert.Equal(1, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenTransitionIsForward_AdvancesStatusUpdatedAt()
        {
            var application = Application();
            var original = application.StatusUpdatedAt;
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            await handler.Handle(Command(JobApplicationStatus.UnderReview), CancellationToken.None);

            Assert.True(application.StatusUpdatedAt > original);
            // AppliedAt is the creation stamp and must not move.
            Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), application.AppliedAt);
        }

        [Fact]
        public async Task Handle_WhenInterviewRejected_IsAllowed()
        {
            var application = Application(JobApplicationStatus.InterView);
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            await handler.Handle(Command(JobApplicationStatus.Rejected), CancellationToken.None);

            Assert.Equal(JobApplicationStatus.Rejected, application.JobApplicationStatus);
        }

        [Fact]
        public async Task Handle_WhenTransitionSkipsAStage_ThrowsBadRequest()
        {
            var application = Application();
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            // Applied -> InterView skips UnderReview.
            await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(Command(JobApplicationStatus.InterView), CancellationToken.None));

            Assert.Equal(JobApplicationStatus.Applied, application.JobApplicationStatus);
            Assert.Equal(0, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenTransitionMovesBackwards_ThrowsBadRequest()
        {
            var application = Application(JobApplicationStatus.UnderReview);
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(Command(JobApplicationStatus.Applied), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenApplicationIsInATerminalState_ThrowsBadRequest()
        {
            var application = Application(JobApplicationStatus.Accepted);
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(Command(JobApplicationStatus.Rejected), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenCallerDoesNotOwnTheJob_ThrowsForbidden()
        {
            var application = Application();
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => handler.Handle(Command(JobApplicationStatus.UnderReview, OtherRecruiterId), CancellationToken.None));

            Assert.Equal(JobApplicationStatus.Applied, application.JobApplicationStatus);
            Assert.Equal(0, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenApplicationDoesNotExist_ThrowsNotFound()
        {
            var repository = new InMemoryJobApplicationRepository();
            var handler = new UpdateApplicationStatusCommandHandler(repository);

            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(Command(JobApplicationStatus.UnderReview), CancellationToken.None));
        }
    }
}

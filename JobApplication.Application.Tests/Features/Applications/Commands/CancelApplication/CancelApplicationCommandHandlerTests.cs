using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Applications.Commands.CancelApplication;
using JobApplication.Application.Tests.Fakes;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Xunit;

namespace JobApplication.Application.Tests.Features.Applications.Commands.CancelApplication
{
    public class CancelApplicationCommandHandlerTests
    {
        private const int CandidateId = 5;
        private const int OtherCandidateId = 88;
        private const int ApplicationId = 3;

        private static JobCandidateApplication Application(JobApplicationStatus status = JobApplicationStatus.Applied)
            => new()
            {
                Id = ApplicationId,
                CandidateId = CandidateId,
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
                    RecruiterId = 7,
                },
            };

        [Theory]
        [InlineData(JobApplicationStatus.Applied)]
        [InlineData(JobApplicationStatus.UnderReview)]
        public async Task Handle_WhenStateAllowsCancellation_CancelsTheApplication(JobApplicationStatus status)
        {
            var application = Application(status);
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new CancelApplicationCommandHandler(repository);

            await handler.Handle(new CancelApplicationCommand(ApplicationId, CandidateId), CancellationToken.None);

            Assert.Equal(JobApplicationStatus.Cancelled, application.JobApplicationStatus);
            Assert.NotNull(application.CancelledAt);
            Assert.Equal(1, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_IsASoftDelete_SoTheRowRemains()
        {
            var application = Application();
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new CancelApplicationCommandHandler(repository);

            await handler.Handle(new CancelApplicationCommand(ApplicationId, CandidateId), CancellationToken.None);

            // The row must survive, otherwise the candidate could apply again
            // for the same job and bypass the duplicate rule.
            Assert.Single(repository.Applications);
        }

        [Fact]
        public async Task Handle_WhenInterviewAlreadyReached_ThrowsBadRequest()
        {
            var application = Application(JobApplicationStatus.InterView);
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new CancelApplicationCommandHandler(repository);

            await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(new CancelApplicationCommand(ApplicationId, CandidateId), CancellationToken.None));

            Assert.Equal(JobApplicationStatus.InterView, application.JobApplicationStatus);
            Assert.Equal(0, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenCallerIsNotTheApplicant_ThrowsForbidden()
        {
            var application = Application();
            var repository = new InMemoryJobApplicationRepository(application);
            var handler = new CancelApplicationCommandHandler(repository);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => handler.Handle(new CancelApplicationCommand(ApplicationId, OtherCandidateId), CancellationToken.None));

            Assert.Equal(JobApplicationStatus.Applied, application.JobApplicationStatus);
            Assert.Equal(0, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenApplicationDoesNotExist_ThrowsNotFound()
        {
            var repository = new InMemoryJobApplicationRepository();
            var handler = new CancelApplicationCommandHandler(repository);

            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(new CancelApplicationCommand(404, CandidateId), CancellationToken.None));
        }
    }
}

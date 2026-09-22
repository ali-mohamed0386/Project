using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Applications.Commands.ApplyForJob;
using JobApplication.Application.Tests.Fakes;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Xunit;

namespace JobApplication.Application.Tests.Features.Applications.Commands.ApplyForJob
{
    public class ApplyForJobCommandHandlerTests
    {
        private const int CandidateId = 5;
        private const int RecruiterId = 7;
        private const int JobId = 1;

        private static Job OpenJob() => new()
        {
            Id = JobId,
            Title = "Backend Developer",
            Description = "Build and maintain the public API.",
            IsActive = true,
            RecruiterId = RecruiterId,
        };

        private static Candidate Candidate() => new()
        {
            Id = CandidateId,
            Name = "Sam Applicant",
            Email = "sam@example.com",
            PasswordHash = "hashed::secret",
        };

        private static ApplyForJobCommandHandler Build(
            InMemoryJobApplicationRepository applications,
            Job? job = null,
            Candidate? candidate = null)
            => new(
                applications,
                new InMemoryJobRepository(job ?? OpenJob()),
                new InMemoryCandidateRepository(candidate ?? Candidate()));

        [Fact]
        public async Task Handle_WhenJobIsOpenAndNotYetApplied_StartsTheApplicationInTheAppliedState()
        {
            var applications = new InMemoryJobApplicationRepository();
            var handler = Build(applications);

            var result = await handler.Handle(
                new ApplyForJobCommand(new CreateApplicationDto { JobId = JobId }, CandidateId),
                CancellationToken.None);

            Assert.Equal(JobId, result.JobId);
            Assert.Equal(CandidateId, result.CandidateId);
            Assert.Equal(JobApplicationStatus.Applied, result.Status);
            Assert.Null(result.CancelledAt);
            Assert.Equal(1, applications.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenJobIsOpenAndNotYetApplied_StampsBothTimestamps()
        {
            var applications = new InMemoryJobApplicationRepository();
            var handler = Build(applications);

            var before = DateTime.UtcNow;
            var result = await handler.Handle(
                new ApplyForJobCommand(new CreateApplicationDto { JobId = JobId }, CandidateId),
                CancellationToken.None);
            var after = DateTime.UtcNow;

            Assert.InRange(result.AppliedAt, before.AddSeconds(-1), after.AddSeconds(1));
            Assert.Equal(result.AppliedAt, result.StatusUpdatedAt);
        }

        [Fact]
        public async Task Handle_WhenJobDoesNotExist_ThrowsNotFound()
        {
            var applications = new InMemoryJobApplicationRepository();
            var handler = new ApplyForJobCommandHandler(
                applications,
                new InMemoryJobRepository(),
                new InMemoryCandidateRepository(Candidate()));

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
                new ApplyForJobCommand(new CreateApplicationDto { JobId = 999 }, CandidateId),
                CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenCandidateDoesNotExist_ThrowsNotFound()
        {
            var applications = new InMemoryJobApplicationRepository();
            var handler = new ApplyForJobCommandHandler(
                applications,
                new InMemoryJobRepository(OpenJob()),
                new InMemoryCandidateRepository());

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
                new ApplyForJobCommand(new CreateApplicationDto { JobId = JobId }, CandidateId),
                CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenJobIsClosed_ThrowsBadRequest()
        {
            var job = OpenJob();
            job.IsActive = false;
            var applications = new InMemoryJobApplicationRepository();
            var handler = Build(applications, job);

            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(
                new ApplyForJobCommand(new CreateApplicationDto { JobId = JobId }, CandidateId),
                CancellationToken.None));

            Assert.Equal(0, applications.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenCandidateAlreadyApplied_ThrowsConflict()
        {
            var applications = new InMemoryJobApplicationRepository(new JobCandidateApplication
            {
                Id = 1,
                CandidateId = CandidateId,
                JobId = JobId,
                JobApplicationStatus = JobApplicationStatus.Applied,
            });
            var handler = Build(applications);

            await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
                new ApplyForJobCommand(new CreateApplicationDto { JobId = JobId }, CandidateId),
                CancellationToken.None));

            Assert.Equal(0, applications.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenCandidatePreviouslyCancelled_StillCountsAsADuplicate()
        {
            // Cancellation is a soft delete precisely so this rule keeps holding:
            // the row is still there, so ExistsAsync still returns true.
            var applications = new InMemoryJobApplicationRepository(new JobCandidateApplication
            {
                Id = 1,
                CandidateId = CandidateId,
                JobId = JobId,
                JobApplicationStatus = JobApplicationStatus.Cancelled,
                CancelledAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });
            var handler = Build(applications);

            await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
                new ApplyForJobCommand(new CreateApplicationDto { JobId = JobId }, CandidateId),
                CancellationToken.None));
        }
    }
}

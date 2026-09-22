using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Tests.Fakes;
using JobApplication.Domain.Entities;
using Xunit;

namespace JobApplication.Application.Tests.Features.Jobs.Commands.CloseJob
{
    public class CloseJobCommandHandlerTests
    {
        private const int OwnerId = 7;
        private const int OtherRecruiterId = 99;

        private static Job OpenJob(int id = 1, int recruiterId = OwnerId) => new()
        {
            Id = id,
            Title = "Backend Developer",
            Description = "Build and maintain the API.",
            IsActive = true,
            RecruiterId = recruiterId,
        };

        [Fact]
        public async Task Handle_WhenJobIsOpenAndOwnedByCaller_ClosesTheJob()
        {
            var job = OpenJob();
            var repository = new InMemoryJobRepository(job);
            var handler = new CloseJobCommandHandler(repository);

            await handler.Handle(new CloseJobCommand(job.Id, OwnerId), CancellationToken.None);

            Assert.False(job.IsActive);
        }

        [Fact]
        public async Task Handle_WhenJobIsOpenAndOwnedByCaller_RecordsTheAuditTrail()
        {
            var job = OpenJob();
            var repository = new InMemoryJobRepository(job);
            var handler = new CloseJobCommandHandler(repository);

            var before = DateTime.UtcNow;
            await handler.Handle(new CloseJobCommand(job.Id, OwnerId), CancellationToken.None);
            var after = DateTime.UtcNow;

            Assert.NotNull(job.ClosedAt);
            Assert.InRange(job.ClosedAt!.Value, before.AddSeconds(-1), after.AddSeconds(1));
            // ClosedBy records who closed it — the caller, not the job owner field.
            Assert.Equal(OwnerId, job.ClosedBy);
        }

        [Fact]
        public async Task Handle_WhenJobIsOpenAndOwnedByCaller_PersistsExactlyOnce()
        {
            var job = OpenJob();
            var repository = new InMemoryJobRepository(job);
            var handler = new CloseJobCommandHandler(repository);

            await handler.Handle(new CloseJobCommand(job.Id, OwnerId), CancellationToken.None);

            Assert.Equal(1, repository.UpdateCallCount);
            Assert.Equal(1, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenJobDoesNotExist_ThrowsNotFound()
        {
            var repository = new InMemoryJobRepository();
            var handler = new CloseJobCommandHandler(repository);

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => handler.Handle(new CloseJobCommand(404, OwnerId), CancellationToken.None));

            Assert.Contains("404", exception.Message);
        }

        [Fact]
        public async Task Handle_WhenCallerDoesNotOwnTheJob_ThrowsForbidden()
        {
            var job = OpenJob();
            var repository = new InMemoryJobRepository(job);
            var handler = new CloseJobCommandHandler(repository);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => handler.Handle(new CloseJobCommand(job.Id, OtherRecruiterId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenJobIsAlreadyClosed_ThrowsBadRequest()
        {
            var job = OpenJob();
            job.IsActive = false;
            var repository = new InMemoryJobRepository(job);
            var handler = new CloseJobCommandHandler(repository);

            await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(new CloseJobCommand(job.Id, OwnerId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenCallerDoesNotOwnTheJob_LeavesTheJobUntouched()
        {
            var job = OpenJob();
            var repository = new InMemoryJobRepository(job);
            var handler = new CloseJobCommandHandler(repository);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => handler.Handle(new CloseJobCommand(job.Id, OtherRecruiterId), CancellationToken.None));

            Assert.True(job.IsActive);
            Assert.Null(job.ClosedAt);
            Assert.Null(job.ClosedBy);
            Assert.Equal(0, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task Handle_WhenJobIsAlreadyClosed_DoesNotPersistAgain()
        {
            var job = OpenJob();
            job.IsActive = false;
            job.ClosedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            job.ClosedBy = OwnerId;
            var closedAt = job.ClosedAt;

            var repository = new InMemoryJobRepository(job);
            var handler = new CloseJobCommandHandler(repository);

            await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(new CloseJobCommand(job.Id, OwnerId), CancellationToken.None));

            // The original audit values must survive the rejected attempt.
            Assert.Equal(closedAt, job.ClosedAt);
            Assert.Equal(0, repository.SaveChangesCallCount);
        }
    }
}

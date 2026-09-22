using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Jobs.Commands.CreateJob;
using JobApplication.Application.Tests.Fakes;
using Xunit;

namespace JobApplication.Application.Tests.Features.Jobs.Commands.CreateJob
{
    public class CreateJobCommandHandlerTests
    {
        private static CreateJobDto ValidDto() => new()
        {
            Title = "Backend Developer",
            Description = "Build and maintain the public API.",
        };

        [Fact]
        public async Task Handle_CreatesAnActiveJobOwnedByTheCaller()
        {
            var repository = new InMemoryJobRepository();
            var handler = new CreateJobCommandHandler(repository);

            var result = await handler.Handle(new CreateJobCommand(ValidDto(), 42), CancellationToken.None);

            // The recruiter id comes from the command (i.e. the token), never the body.
            Assert.Equal(42, result.RecruiterId);
            Assert.True(result.IsActive);
            Assert.Equal("Backend Developer", result.Title);
            Assert.Equal("Build and maintain the public API.", result.Description);
            Assert.Null(result.ClosedAt);
            Assert.Null(result.ClosedBy);
        }

        [Fact]
        public async Task Handle_PersistsTheNewJob()
        {
            var repository = new InMemoryJobRepository();
            var handler = new CreateJobCommandHandler(repository);

            var result = await handler.Handle(new CreateJobCommand(ValidDto(), 42), CancellationToken.None);

            var stored = Assert.Single(repository.Jobs);
            Assert.Equal(result.Id, stored.Id);
            Assert.True(stored.Id > 0);
            Assert.Equal(1, repository.SaveChangesCallCount);
        }
    }
}

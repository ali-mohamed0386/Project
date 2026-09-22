using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Auth.Commands.Login;
using JobApplication.Application.Tests.Fakes;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Xunit;

namespace JobApplication.Application.Tests.Features.Auth.Commands.Login
{
    public class LoginCommandHandlerTests
    {
        private static Candidate Candidate() => new()
        {
            Id = 5,
            Name = "Sam Applicant",
            Email = "sam@example.com",
            PasswordHash = "hashed::secret123",
        };

        private static Recruiter Recruiter() => new()
        {
            Id = 9,
            Name = "Rita Recruiter",
            Email = "hr@example.com",
            PasswordHash = "hashed::secret123",
        };

        private static LoginCommandHandler Build(
            InMemoryCandidateRepository? candidates = null,
            InMemoryRecruiterRepository? recruiters = null)
            => new(
                candidates ?? new InMemoryCandidateRepository(Candidate()),
                recruiters ?? new InMemoryRecruiterRepository(Recruiter()),
                new FakePasswordHasher(),
                new FakeTokenService());

        private static LoginDto Dto(string email, string password = "secret123", string? role = null)
            => new() { Email = email, Password = password, Role = role };

        [Fact]
        public async Task Handle_WithCandidateRole_ReturnsACandidateToken()
        {
            var handler = Build();

            var result = await handler.Handle(
                new LoginCommand(Dto("sam@example.com", role: UserRoles.Candidate)), CancellationToken.None);

            Assert.Equal(UserRoles.Candidate, result.Role);
            Assert.Equal(5, result.UserId);
            Assert.Equal("token-for:5:Candidate", result.Token);
        }

        [Fact]
        public async Task Handle_WithRecruiterRole_ReturnsARecruiterToken()
        {
            var handler = Build();

            var result = await handler.Handle(
                new LoginCommand(Dto("hr@example.com", role: UserRoles.Recruiter)), CancellationToken.None);

            Assert.Equal(UserRoles.Recruiter, result.Role);
            Assert.Equal(9, result.UserId);
        }

        [Fact]
        public async Task Handle_WithoutRole_FindsACandidate()
        {
            var handler = Build();

            var result = await handler.Handle(new LoginCommand(Dto("sam@example.com")), CancellationToken.None);

            Assert.Equal(UserRoles.Candidate, result.Role);
        }

        [Fact]
        public async Task Handle_WithoutRole_FallsBackToRecruiters()
        {
            var handler = Build();

            var result = await handler.Handle(new LoginCommand(Dto("hr@example.com")), CancellationToken.None);

            Assert.Equal(UserRoles.Recruiter, result.Role);
        }

        [Fact]
        public async Task Handle_NormalisesTheEmailBeforeLookingItUp()
        {
            var handler = Build();

            var result = await handler.Handle(
                new LoginCommand(Dto("  Sam@Example.COM  ")), CancellationToken.None);

            Assert.Equal(UserRoles.Candidate, result.Role);
        }

        [Fact]
        public async Task Handle_WhenPasswordIsWrong_ThrowsUnauthorizedWithAGenericMessage()
        {
            var handler = Build();

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(new LoginCommand(Dto("sam@example.com", password: "wrong-password")), CancellationToken.None));

            Assert.Equal("Email or password is incorrect.", exception.Message);
        }

        [Fact]
        public async Task Handle_WhenEmailIsUnknown_ThrowsTheSameUnauthorizedMessage()
        {
            // SECURITY: an unknown email and a wrong password must be
            // indistinguishable, otherwise the endpoint leaks which addresses
            // are registered.
            var handler = Build();

            var unknownEmail = await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(new LoginCommand(Dto("nobody@example.com")), CancellationToken.None));

            var wrongPassword = await Assert.ThrowsAsync<UnauthorizedException>(
                () => handler.Handle(new LoginCommand(Dto("sam@example.com", password: "wrong")), CancellationToken.None));

            Assert.Equal(wrongPassword.Message, unknownEmail.Message);
        }

        [Fact]
        public async Task Handle_WhenCandidateIsSearchedByRoleButOnlyARecruiterExists_ThrowsUnauthorized()
        {
            var handler = Build(
                candidates: new InMemoryCandidateRepository(),
                recruiters: new InMemoryRecruiterRepository(Recruiter()));

            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(
                new LoginCommand(Dto("hr@example.com", role: UserRoles.Candidate)), CancellationToken.None));
        }
    }
}

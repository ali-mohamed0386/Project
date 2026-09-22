using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Auth.Commands.Register;
using JobApplication.Application.Tests.Fakes;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Xunit;

namespace JobApplication.Application.Tests.Features.Auth.Commands.Register
{
    public class RegisterCommandHandlerTests
    {
        private static RegisterDto Dto(
            string role = UserRoles.Candidate,
            string email = "sam@example.com",
            string name = "Sam Applicant",
            string password = "secret123",
            string? cvUrl = null) => new()
            {
                Name = name,
                Email = email,
                Password = password,
                Role = role,
                CvUrl = cvUrl,
            };

        private static (RegisterCommandHandler Handler, InMemoryCandidateRepository Candidates, InMemoryRecruiterRepository Recruiters) Build(
            InMemoryCandidateRepository? candidates = null,
            InMemoryRecruiterRepository? recruiters = null)
        {
            candidates ??= new InMemoryCandidateRepository();
            recruiters ??= new InMemoryRecruiterRepository();

            return (new RegisterCommandHandler(
                candidates,
                recruiters,
                new FakePasswordHasher(),
                new FakeTokenService()), candidates, recruiters);
        }

        [Fact]
        public async Task Handle_WithCandidateRole_CreatesACandidateAndReturnsAToken()
        {
            var (handler, candidates, recruiters) = Build();

            var result = await handler.Handle(new RegisterCommand(Dto()), CancellationToken.None);

            Assert.Equal(UserRoles.Candidate, result.Role);
            Assert.Equal("sam@example.com", result.Email);
            Assert.Equal("Sam Applicant", result.Name);
            Assert.StartsWith("token-for:", result.Token);
            Assert.Single(candidates.Candidates);
            Assert.Empty(recruiters.Recruiters);
        }

        [Fact]
        public async Task Handle_WithRecruiterRole_CreatesARecruiterAndReturnsAToken()
        {
            var (handler, candidates, recruiters) = Build();

            var result = await handler.Handle(
                new RegisterCommand(Dto(role: UserRoles.Recruiter, email: "hr@example.com", name: "Rita Recruiter")),
                CancellationToken.None);

            Assert.Equal(UserRoles.Recruiter, result.Role);
            Assert.StartsWith("token-for:", result.Token);
            Assert.Single(recruiters.Recruiters);
            Assert.Empty(candidates.Candidates);
        }

        [Fact]
        public async Task Handle_StoresOnlyThePasswordHash()
        {
            var (handler, candidates, _) = Build();

            await handler.Handle(new RegisterCommand(Dto(password: "secret123")), CancellationToken.None);

            var stored = Assert.Single(candidates.Candidates);
            Assert.DoesNotContain("secret123", stored.PasswordHash);
            Assert.Equal("hashed::secret123", stored.PasswordHash);
        }

        [Fact]
        public async Task Handle_NormalisesTheEmailBeforeStoring()
        {
            var (handler, candidates, _) = Build();

            await handler.Handle(new RegisterCommand(Dto(email: "  Sam@Example.COM  ")), CancellationToken.None);

            var stored = Assert.Single(candidates.Candidates);
            Assert.Equal("sam@example.com", stored.Email);
        }

        [Fact]
        public async Task Handle_WhenCvUrlIsOmitted_StoresAnEmptyString()
        {
            // CvUrl is a NOT NULL column, so it needs a value even when the
            // caller does not supply one.
            var (handler, candidates, _) = Build();

            await handler.Handle(new RegisterCommand(Dto(cvUrl: null)), CancellationToken.None);

            Assert.Equal(string.Empty, Assert.Single(candidates.Candidates).CvUrl);
        }

        [Fact]
        public async Task Handle_WhenEmailIsAlreadyRegisteredAsCandidate_ThrowsConflict()
        {
            var candidates = new InMemoryCandidateRepository(new Candidate
            {
                Id = 1,
                Name = "Existing",
                Email = "sam@example.com",
                PasswordHash = "hashed::whatever",
            });
            var (handler, _, _) = Build(candidates);

            await Assert.ThrowsAsync<ConflictException>(
                () => handler.Handle(new RegisterCommand(Dto()), CancellationToken.None));

            Assert.Single(candidates.Candidates);
        }

        [Fact]
        public async Task Handle_WhenEmailIsAlreadyRegisteredAsRecruiter_ThrowsConflict()
        {
            var recruiters = new InMemoryRecruiterRepository(new Recruiter
            {
                Id = 1,
                Name = "Existing",
                Email = "hr@example.com",
                PasswordHash = "hashed::whatever",
            });
            var (handler, _, _) = Build(recruiters: recruiters);

            await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
                new RegisterCommand(Dto(role: UserRoles.Recruiter, email: "hr@example.com")),
                CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenRoleIsNotRecognised_ThrowsBadRequest()
        {
            var (handler, candidates, recruiters) = Build();

            await Assert.ThrowsAsync<BadRequestException>(
                () => handler.Handle(new RegisterCommand(Dto(role: "Admin")), CancellationToken.None));

            Assert.Empty(candidates.Candidates);
            Assert.Empty(recruiters.Recruiters);
        }

        [Fact]
        public async Task Handle_WhenRoleDiffersOnlyByCasing_StillRegisters()
        {
            // The role check is OrdinalIgnoreCase, so "candidate" is accepted.
            var (handler, candidates, _) = Build();

            await handler.Handle(new RegisterCommand(Dto(role: "candidate")), CancellationToken.None);

            Assert.Single(candidates.Candidates);
        }
    }
}

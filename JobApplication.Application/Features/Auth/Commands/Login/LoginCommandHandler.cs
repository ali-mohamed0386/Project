using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Auth;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IRecruiterRepository _recruiterRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(
            ICandidateRepository candidateRepository,
            IRecruiterRepository recruiterRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService)
        {
            _candidateRepository = candidateRepository;
            _recruiterRepository = recruiterRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Credentials;

            var email = dto.Email.Trim().ToLowerInvariant();

            // When a role is supplied, only that table is searched.
            if (string.Equals(dto.Role?.Trim(), UserRoles.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                var candidate = await _candidateRepository.GetByEmailAsync(email)
                    ?? throw InvalidCredentials();

                if (!_passwordHasher.Verify(dto.Password, candidate.PasswordHash))
                    throw InvalidCredentials();

                return AuthResponseFactory.Build(
                    _tokenService, candidate.Id, UserRoles.Candidate, candidate.Email, candidate.Name);
            }

            if (string.Equals(dto.Role?.Trim(), UserRoles.Recruiter, StringComparison.OrdinalIgnoreCase))
            {
                var recruiter = await _recruiterRepository.GetByEmailAsync(email)
                    ?? throw InvalidCredentials();

                if (!_passwordHasher.Verify(dto.Password, recruiter.PasswordHash))
                    throw InvalidCredentials();

                return AuthResponseFactory.Build(
                    _tokenService, recruiter.Id, UserRoles.Recruiter, recruiter.Email, recruiter.Name);
            }

            // No role supplied (or an unknown value) → search both tables.
            var candidateUser = await _candidateRepository.GetByEmailAsync(email);

            if (candidateUser is not null)
            {
                if (!_passwordHasher.Verify(dto.Password, candidateUser.PasswordHash))
                    throw InvalidCredentials();

                return AuthResponseFactory.Build(
                    _tokenService, candidateUser.Id, UserRoles.Candidate, candidateUser.Email, candidateUser.Name);
            }

            var recruiterUser = await _recruiterRepository.GetByEmailAsync(email);

            if (recruiterUser is not null)
            {
                if (!_passwordHasher.Verify(dto.Password, recruiterUser.PasswordHash))
                    throw InvalidCredentials();

                return AuthResponseFactory.Build(
                    _tokenService, recruiterUser.Id, UserRoles.Recruiter, recruiterUser.Email, recruiterUser.Name);
            }

            throw InvalidCredentials();
        }

        /// <summary>
        /// SECURITY: the exact same message is returned for every failure mode —
        /// whether the email does not exist or the password is wrong. Returning
        /// different messages would reveal which email addresses are registered.
        /// </summary>
        private static UnauthorizedException InvalidCredentials()
            => new("Email or password is incorrect.");
    }
}

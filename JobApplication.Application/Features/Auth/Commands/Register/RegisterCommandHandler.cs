using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Features.Auth;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Register
{
    public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IRecruiterRepository _recruiterRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public RegisterCommandHandler(
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

        public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Account;

            var role = dto.Role.Trim();
            // Normalize the email (lowercase + trim) so the same address cannot
            // be registered twice with different casing, and so the lookup
            // during login works reliably.
            var email = dto.Email.Trim().ToLowerInvariant();

            if (string.Equals(role, UserRoles.Candidate, StringComparison.OrdinalIgnoreCase))
            {
                if (await _candidateRepository.EmailExistsAsync(email))
                    throw new ConflictException("This email is already registered as a Candidate.");

                var candidate = new Candidate
                {
                    Name = dto.Name.Trim(),
                    Email = email,
                    // SECURITY: only the hash is stored — never the password itself.
                    PasswordHash = _passwordHasher.Hash(dto.Password),
                    // CvUrl is an existing NOT NULL column, so it needs an
                    // initial value.
                    CvUrl = dto.CvUrl ?? string.Empty,
                };

                await _candidateRepository.InsertAsync(candidate);
                await _candidateRepository.SaveChangesAsync();

                return AuthResponseFactory.Build(
                    _tokenService, candidate.Id, UserRoles.Candidate, candidate.Email, candidate.Name);
            }

            if (string.Equals(role, UserRoles.Recruiter, StringComparison.OrdinalIgnoreCase))
            {
                if (await _recruiterRepository.EmailExistsAsync(email))
                    throw new ConflictException("This email is already registered as a Recruiter.");

                var recruiter = new Recruiter
                {
                    Name = dto.Name.Trim(),
                    Email = email,
                    PasswordHash = _passwordHasher.Hash(dto.Password),
                };

                await _recruiterRepository.InsertAsync(recruiter);
                await _recruiterRepository.SaveChangesAsync();

                return AuthResponseFactory.Build(
                    _tokenService, recruiter.Id, UserRoles.Recruiter, recruiter.Email, recruiter.Name);
            }

            throw new BadRequestException(
                $"Role must be either '{UserRoles.Candidate}' or '{UserRoles.Recruiter}'.");
        }
    }
}

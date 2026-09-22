using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Register
{
    /// <summary>
    /// Registers a new account (Candidate or Recruiter, depending on the
    /// requested role) and returns a token immediately, so the caller does not
    /// have to log in afterwards.
    /// </summary>
    public sealed record RegisterCommand(RegisterDto Account) : IRequest<AuthResponseDto>;
}

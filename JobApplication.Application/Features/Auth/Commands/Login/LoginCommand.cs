using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Signs in an existing account and issues a token.
    /// </summary>
    public sealed record LoginCommand(LoginDto Credentials) : IRequest<AuthResponseDto>;
}

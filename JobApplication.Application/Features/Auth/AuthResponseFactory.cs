using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Features.Auth
{
    /// <summary>
    /// Builds the authentication response shared by the Register and Login
    /// handlers. Kept in one place so the token projection cannot drift
    /// between the two flows.
    /// </summary>
    internal static class AuthResponseFactory
    {
        public static AuthResponseDto Build(
            ITokenService tokenService,
            int userId,
            string role,
            string email,
            string name)
        {
            var token = tokenService.GenerateToken(userId, role, email, name);

            return new AuthResponseDto
            {
                Token = token.Token,
                ExpiresAtUtc = token.ExpiresAtUtc,
                UserId = userId,
                Name = name,
                Email = email,
                Role = role,
            };
        }
    }
}

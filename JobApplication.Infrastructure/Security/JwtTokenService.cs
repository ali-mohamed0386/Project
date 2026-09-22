using JobApplication.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobApplication.Infrastructure.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public TokenResult GenerateToken(int userId, string role, string email, string name)
        {
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

            var claims = new List<Claim>
            {
                // These claims are how the system identifies "who" the user is
                // on every subsequent request.
                new(ClaimTypes.NameIdentifier, userId.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Role, role),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Name, name),

                // Jti = a unique identifier for the token itself. Useful for
                // revocation later on.
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));

            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }
    }
}

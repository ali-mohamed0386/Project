using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Tests.Fakes
{
    /// <summary>
    /// Produces a predictable token so the tests can assert on the projected
    /// AuthResponseDto without pulling in JWT signing.
    /// </summary>
    internal sealed class FakeTokenService : ITokenService
    {
        public static readonly DateTime Expiry = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public TokenResult GenerateToken(int userId, string role, string email, string name)
            => new($"token-for:{userId}:{role}", Expiry);
    }
}

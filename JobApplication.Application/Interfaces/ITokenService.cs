namespace JobApplication.Application.Interfaces
{
    /// <summary>The result of generating a token.</summary>
    public sealed record TokenResult(string Token, DateTime ExpiresAtUtc);

    /// <summary>
    /// An abstraction over token generation. The Application layer knows that a
    /// token is produced, but not that it is a JWT nor how it is signed —
    /// that is an Infrastructure concern.
    /// </summary>
    public interface ITokenService
    {
        TokenResult GenerateToken(int userId, string role, string email, string name);
    }
}

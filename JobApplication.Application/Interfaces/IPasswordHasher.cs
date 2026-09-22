namespace JobApplication.Application.Interfaces
{
    /// <summary>
    /// An abstraction over password hashing. The Application layer does not
    /// need to know whether PBKDF2, BCrypt, or anything else is used.
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string password);

        bool Verify(string password, string passwordHash);
    }
}

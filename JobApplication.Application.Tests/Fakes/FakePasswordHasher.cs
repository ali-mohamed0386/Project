using JobApplication.Application.Interfaces;

namespace JobApplication.Application.Tests.Fakes
{
    /// <summary>
    /// A deterministic, reversible stand-in for PBKDF2 hashing. Real hashing is
    /// deliberately slow; the tests only care that a hash is produced and that
    /// Verify accepts the matching password and rejects everything else.
    /// </summary>
    internal sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed::{password}";

        public bool Verify(string password, string passwordHash)
            => passwordHash == Hash(password);
    }
}

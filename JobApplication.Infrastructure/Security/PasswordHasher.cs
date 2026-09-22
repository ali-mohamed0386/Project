using JobApplication.Application.Interfaces;
using System.Security.Cryptography;

namespace JobApplication.Infrastructure.Security
{
    /// <summary>
    /// Stores passwords using PBKDF2 (HMAC-SHA256) from the .NET base class
    /// library — no additional NuGet package required.
    /// </summary>
    /// <remarks>
    /// Why PBKDF2 and not plain SHA256?
    /// SHA256 is extremely fast, which is a liability here: an attacker can try
    /// billions of passwords per second. PBKDF2 deliberately makes every attempt
    /// expensive (slow), which throttles brute-force attacks.
    ///
    /// Stored format: {iterations}.{salt base64}.{hash base64}
    /// The iteration count is stored alongside the hash so it can be raised in
    /// the future without invalidating existing passwords.
    /// </remarks>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSizeInBytes = 16;   // 128-bit salt
        private const int KeySizeInBytes = 32;    // 256-bit derived key
        private const int Iterations = 100_000;

        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            // A random salt per user — so that two users with the same password
            // end up with different hashes. This defeats rainbow tables.
            var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);

            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySizeInBytes);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool Verify(string password, string passwordHash)
        {
            var parts = passwordHash.Split('.', 3);

            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out var iterations) || iterations <= 0)
                return false;

            byte[] salt;
            byte[] expectedKey;

            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedKey = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterations, Algorithm, expectedKey.Length);

            // SECURITY: FixedTimeEquals instead of ==.
            // A normal comparison short-circuits at the first differing byte,
            // which leaks information about the hash through timing differences
            // (a timing attack). This one takes a constant amount of time
            // regardless of how similar the inputs are.
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
    }
}

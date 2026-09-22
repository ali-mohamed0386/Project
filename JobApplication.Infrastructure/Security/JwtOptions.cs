namespace JobApplication.Infrastructure.Security
{
    /// <summary>
    /// JWT settings — bound from the "Jwt" section of configuration.
    /// </summary>
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// The secret key used to sign the token. It must be at least 32
        /// characters (256-bit) for HMAC-SHA256.
        /// </summary>
        public string Secret { get; set; } = string.Empty;

        public int ExpiryMinutes { get; set; } = 60;
    }
}

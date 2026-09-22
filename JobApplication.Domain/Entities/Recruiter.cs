namespace JobApplication.Domain.Entities
{
    public class Recruiter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Login credentials
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}

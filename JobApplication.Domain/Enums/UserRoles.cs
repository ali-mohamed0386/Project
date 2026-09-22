namespace JobApplication.Domain.Enums
{
    /// <summary>
    /// Role names used in the JWT claim and in [Authorize(Roles = ...)].
    /// Declared as constants to avoid magic strings across the codebase.
    /// </summary>
    public static class UserRoles
    {
        public const string Candidate = "Candidate";
        public const string Recruiter = "Recruiter";
    }
}

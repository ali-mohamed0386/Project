namespace JobApplication.Application.Interfaces
{
    /// <summary>
    /// The source of the current identity — read from the JWT claims of the
    /// incoming request.
    /// </summary>
    /// <remarks>
    /// SECURITY RULE: ownership ids (CandidateId, RecruiterId) must be taken
    /// from here — from the signed token — never from the request body.
    /// </remarks>
    public interface ICurrentUserService
    {
        /// <summary>The id of the current user, or null when unauthenticated.</summary>
        int? UserId { get; }

        /// <summary>The role of the current user (Candidate / Recruiter).</summary>
        string? Role { get; }

        /// <summary>
        /// Same as <see cref="UserId"/> but throws UnauthorizedException when
        /// no identity is present. Used by controllers to avoid a
        /// NullReferenceException.
        /// </summary>
        int GetRequiredUserId();
    }
}

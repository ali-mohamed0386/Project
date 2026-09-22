using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    /// <summary>
    /// Closes an open job posting. Only the recruiter who owns the job may
    /// close it.
    /// </summary>
    /// <remarks>
    /// SECURITY: <paramref name="RecruiterId"/> comes from the JWT claim, and is
    /// compared against the job's owner inside the handler.
    /// </remarks>
    public sealed record CloseJobCommand(int JobId, int RecruiterId) : IRequest;
}

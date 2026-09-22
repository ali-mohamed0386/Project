using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.CancelApplication
{
    /// <summary>
    /// Cancels an application on behalf of the applicant. This is not a
    /// physical delete — the status is set to Cancelled and CancelledAt is
    /// recorded.
    /// </summary>
    /// <remarks>
    /// SECURITY: <paramref name="CandidateId"/> comes from the JWT claim, and is
    /// compared against the application's owner inside the handler.
    /// </remarks>
    public sealed record CancelApplicationCommand(int ApplicationId, int CandidateId) : IRequest;
}

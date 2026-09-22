using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.ApplyForJob
{
    /// <summary>
    /// Submits an application for an open job on behalf of a candidate.
    /// </summary>
    /// <remarks>
    /// SECURITY: <paramref name="CandidateId"/> is read from the JWT claim by
    /// the controller, never from the request body. Accepting it from the body
    /// would let anyone apply on behalf of someone else (IDOR).
    /// </remarks>
    public sealed record ApplyForJobCommand(CreateApplicationDto Application, int CandidateId)
        : IRequest<ApplicationResponseDto>;
}

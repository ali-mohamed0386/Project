using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CreateJob
{
    /// <summary>
    /// Creates a new job posting owned by the recruiter who issued the request.
    /// </summary>
    /// <remarks>
    /// SECURITY: <paramref name="RecruiterId"/> is resolved from the JWT claim
    /// by the controller and passed in here — it is never taken from the
    /// request body.
    /// </remarks>
    public sealed record CreateJobCommand(CreateJobDto Job, int RecruiterId) : IRequest<JobResponseDto>;
}

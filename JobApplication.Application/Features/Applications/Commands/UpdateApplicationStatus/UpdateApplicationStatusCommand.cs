using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.UpdateApplicationStatus
{
    /// <summary>
    /// Moves an application to a new status. The caller must own the job the
    /// application was submitted for.
    /// </summary>
    /// <remarks>
    /// SECURITY: <paramref name="RecruiterId"/> comes from the JWT claim, and is
    /// compared against the owning job's RecruiterId inside the handler.
    /// </remarks>
    public sealed record UpdateApplicationStatusCommand(
        int ApplicationId,
        UpdateApplicationStatusDto Status,
        int RecruiterId) : IRequest;
}

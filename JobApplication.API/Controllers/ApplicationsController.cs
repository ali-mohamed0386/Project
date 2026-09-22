using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Applications.Commands.CancelApplication;
using JobApplication.Application.Features.Applications.Commands.ApplyForJob;
using JobApplication.Application.Features.Applications.Commands.UpdateApplicationStatus;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    /// <summary>
    /// Job applications.
    /// Note the name ApplicationsController (not JobCandidateApplicationsController):
    /// the [controller] token generates the required route, /api/applications.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public ApplicationsController(IMediator mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>Applies for a job. Candidates only.</summary>
        /// <remarks>
        /// The applying candidate is taken from the bearer token, so there is no
        /// CandidateId field in the request body — this makes it impossible to
        /// apply on behalf of someone else.
        /// </remarks>
        /// <param name="createApplicationDto">The id of the job to apply for.</param>
        /// <response code="201">The application was created.</response>
        /// <response code="400">The job is closed and no longer accepts applications.</response>
        /// <response code="401">The token is missing or invalid.</response>
        /// <response code="403">The caller is not a Candidate.</response>
        /// <response code="404">The job or the candidate does not exist.</response>
        /// <response code="409">The candidate has already applied for this job.</response>
        [HttpPost]
        [Authorize(Roles = UserRoles.Candidate)]
        [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Apply([FromBody] CreateApplicationDto createApplicationDto)
        {
            // SECURITY: the CandidateId comes from the token — not from the
            // body. This makes it impossible for anyone to apply on behalf of
            // someone else (IDOR).
            var candidateId = _currentUser.GetRequiredUserId();

            var application = await _mediator.Send(new ApplyForJobCommand(createApplicationDto, candidateId));

            return StatusCode(StatusCodes.Status201Created, application);
        }

        /// <summary>Changes an application status. The caller must own the job.</summary>
        /// <remarks>
        /// Only forward transitions are allowed, following the state machine
        /// Applied → UnderReview → InterView → Accepted/Rejected.
        /// </remarks>
        /// <param name="id">The id of the application to update.</param>
        /// <param name="dto">The new status.</param>
        /// <response code="204">The status was updated.</response>
        /// <response code="400">The requested transition is not allowed.</response>
        /// <response code="401">The token is missing or invalid.</response>
        /// <response code="403">The caller is not a Recruiter, or does not own the job.</response>
        /// <response code="404">No application with the supplied id exists.</response>
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = UserRoles.Recruiter)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusDto dto)
        {
            var recruiterId = _currentUser.GetRequiredUserId();

            await _mediator.Send(new UpdateApplicationStatusCommand(id, dto, recruiterId));

            return NoContent();
        }

        /// <summary>
        /// Cancels the application. Only the applicant may cancel it.
        /// This is not a physical delete — the status is set to Cancelled and
        /// CancelledAt is recorded.
        /// </summary>
        /// <param name="id">The id of the application to cancel.</param>
        /// <response code="204">The application was cancelled.</response>
        /// <response code="400">
        /// The application is past the UnderReview state and can no longer be cancelled.
        /// </response>
        /// <response code="401">The token is missing or invalid.</response>
        /// <response code="403">The caller is not a Candidate, or is not the applicant.</response>
        /// <response code="404">No application with the supplied id exists.</response>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Candidate)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(int id)
        {
            var candidateId = _currentUser.GetRequiredUserId();

            await _mediator.Send(new CancelApplicationCommand(id, candidateId));

            return NoContent();
        }
    }
}

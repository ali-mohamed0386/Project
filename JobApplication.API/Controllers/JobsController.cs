using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Features.Jobs.Commands.CreateJob;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public JobsController(IMediator mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>Creates a new job. Recruiters only.</summary>
        /// <remarks>
        /// The owning recruiter is taken from the bearer token, so there is no
        /// RecruiterId field in the request body.
        /// </remarks>
        /// <param name="createJobDto">The title and description of the new job.</param>
        /// <response code="201">The job was created. The created job is returned.</response>
        /// <response code="400">The payload failed validation.</response>
        /// <response code="401">The token is missing or invalid.</response>
        /// <response code="403">The caller is not a Recruiter.</response>
        [HttpPost]
        [Authorize(Roles = UserRoles.Recruiter)]
        [ProducesResponseType(typeof(JobResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateJobDto createJobDto)
        {
            // The RecruiterId comes from the token — not from the request body.
            var recruiterId = _currentUser.GetRequiredUserId();

            var job = await _mediator.Send(new CreateJobCommand(createJobDto, recruiterId));

            // Note: CreatedAtAction requires a GET endpoint to build the
            // Location header. There is no GET /api/jobs/{id} in the project
            // yet, so 201 is returned directly.
            return StatusCode(StatusCodes.Status201Created, job);
        }

        /// <summary>Closes the job. Only its owner may close it.</summary>
        /// <remarks>
        /// Sets the job to inactive and records ClosedAt and ClosedBy.
        /// The caller is identified by the bearer token and must be the
        /// recruiter who owns the job.
        ///
        /// A job may only be closed once — closing it again returns 400.
        /// </remarks>
        /// <param name="id">The id of the job to close.</param>
        /// <response code="204">The job was closed.</response>
        /// <response code="400">The job is already closed.</response>
        /// <response code="401">The token is missing or invalid.</response>
        /// <response code="403">The caller is not a Recruiter, or does not own the job.</response>
        /// <response code="404">No job with the supplied id exists.</response>
        [HttpPut("{id:int}/close")]
        [Authorize(Roles = UserRoles.Recruiter)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Close(int id)
        {
            var recruiterId = _currentUser.GetRequiredUserId();

            await _mediator.Send(new CloseJobCommand(id, recruiterId));

            return NoContent();
        }
    }
}

using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Auth.Commands.Login;
using JobApplication.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Registers a new account (Candidate or Recruiter, depending on the
        /// role) and returns a token immediately, so the caller does not have
        /// to log in afterwards.
        /// </summary>
        /// <param name="registerDto">The account details, including the role.</param>
        /// <response code="201">The account was created and a token issued.</response>
        /// <response code="400">The payload is invalid, or the role is not recognised.</response>
        /// <response code="409">The email address is already registered for that role.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _mediator.Send(new RegisterCommand(registerDto));

            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>Signs in and issues a token.</summary>
        /// <remarks>
        /// When the optional role is supplied, only that table is searched.
        /// When it is omitted, both Candidates and Recruiters are searched.
        /// </remarks>
        /// <param name="loginDto">The credentials, and an optional role.</param>
        /// <response code="200">The credentials were accepted and a token issued.</response>
        /// <response code="400">The payload failed validation.</response>
        /// <response code="401">The email or password is incorrect.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _mediator.Send(new LoginCommand(loginDto));

            return Ok(result);
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs
{
    public class CreateApplicationDto
    {
        [Required(ErrorMessage = "Job id is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Job id is not valid.")]
        public int JobId { get; set; }

        // NOTE: there is deliberately no CandidateId here.
        // The candidate id is read from the JWT claim inside the service,
        // never from the request body. Accepting it from the body would let
        // anyone apply on behalf of someone else (IDOR).
    }
}

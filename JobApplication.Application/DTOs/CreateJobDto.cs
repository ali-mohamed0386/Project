using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs
{
    public class CreateJobDto
    {
        [Required(ErrorMessage = "Job title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Job title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job description is required.")]
        [StringLength(4000, MinimumLength = 10, ErrorMessage = "Job description must be between 10 and 4000 characters.")]
        public string Description { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is not valid.")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Optional. When supplied, the lookup is restricted to that table.
        /// When omitted, both the Candidates and the Recruiters tables are searched.
        /// </summary>
        public string? Role { get; set; }
    }
}

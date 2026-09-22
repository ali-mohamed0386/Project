using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace JobApplication.Application.DTOs
{
    public class UpdateApplicationStatusDto
    {
        [Required(ErrorMessage = "The new status is required.")]
        [EnumDataType(typeof(JobApplicationStatus), ErrorMessage = "The status value is not valid.")]
        public JobApplicationStatus Status { get; set; }
    }
}

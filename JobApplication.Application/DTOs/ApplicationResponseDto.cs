using JobApplication.Domain.Enums;

namespace JobApplication.Application.DTOs
{
    public class ApplicationResponseDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int CandidateId { get; set; }
        public JobApplicationStatus Status { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}

namespace JobApplication.Domain.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Job state. Set to false when the owning recruiter closes the job.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The owner of the job. This is the basis for every ownership check in the system.
        /// </summary>
        public int RecruiterId { get; set; }
        public Recruiter? Recruiter { get; set; }

        /// <summary>
        /// The UTC timestamp when the job was closed. Stays null while the job is open.
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// The recruiter who closed the job (audit trail).
        /// </summary>
        public int? ClosedBy { get; set; }
    }
}

using JobApplication.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities
{
    public class JobCandidateApplication
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        public Candidate Candidate { get; set; } = null!;

        public int JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; } = null!;

        public JobApplicationStatus JobApplicationStatus { get; set; }

        /// <summary>
        /// This is the CreatedAt timestamp required by Day 3. The column already
        /// exists under this name, so no duplicate column was introduced.
        /// </summary>
        public DateTime AppliedAt { get; set; }

        public DateTime StatusUpdatedAt { get; set; }

        /// <summary>
        /// The UTC timestamp when the application was cancelled.
        /// Stays null while the application is active.
        /// </summary>
        public DateTime? CancelledAt { get; set; }
    }
}

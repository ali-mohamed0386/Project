using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface IJobApplicationRepository
    {
        /// <summary>
        /// Loads the application together with its Job (Include) so that the
        /// recruiter's ownership of that job can be verified.
        /// </summary>
        Task<JobCandidateApplication?> GetByIdWithJobAsync(int id);

        /// <summary>Has this candidate already applied for this job?</summary>
        Task<bool> ExistsAsync(int candidateId, int jobId);

        Task InsertAsync(JobCandidateApplication application);
        void Update(JobCandidateApplication application);
        Task SaveChangesAsync();
    }
}

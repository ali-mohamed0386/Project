using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobCandidateApplication?> GetByIdWithJobAsync(int id)
        {
            // The Include is required: without it the job's RecruiterId would
            // not be available, and ownership of the job could not be verified.
            // It also avoids an N+1 query because everything is fetched in a
            // single round trip.
            return await _context.JobCandidateApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> ExistsAsync(int candidateId, int jobId)
        {
            // AnyAsync is faster than Count() > 0 — it stops at the first row.
            return await _context.JobCandidateApplications
                .AsNoTracking()
                .AnyAsync(a => a.CandidateId == candidateId && a.JobId == jobId);
        }

        public async Task InsertAsync(JobCandidateApplication application)
        {
            await _context.JobCandidateApplications.AddAsync(application);
        }

        public void Update(JobCandidateApplication application)
        {
            // Deliberately NOT using _context.JobCandidateApplications.Update(application).
            //
            // DbSet.Update() walks the entire object graph and marks every entity
            // reachable through navigation properties as Modified. Because
            // GetByIdWithJobAsync includes the Job, the Job would be marked
            // Modified as well, which means:
            //   1) A redundant UPDATE against the Jobs table on every call.
            //   2) A worse problem: if the job were modified concurrently, the
            //      stale values read above would be written over it (lost update).
            //
            // Entry(...).State = Modified marks only this entity and leaves the
            // Job untouched.
            _context.Entry(application).State = EntityState.Modified;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

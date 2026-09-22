using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Tests.Fakes
{
    internal sealed class InMemoryJobApplicationRepository : IJobApplicationRepository
    {
        private readonly List<JobCandidateApplication> _applications = new();
        private int _nextId;

        public InMemoryJobApplicationRepository(params JobCandidateApplication[] seed)
        {
            _applications.AddRange(seed);
            _nextId = _applications.Count == 0 ? 0 : _applications.Max(a => a.Id);
        }

        public int SaveChangesCallCount { get; private set; }

        public IReadOnlyList<JobCandidateApplication> Applications => _applications;

        public Task<JobCandidateApplication?> GetByIdWithJobAsync(int id)
            => Task.FromResult(_applications.FirstOrDefault(a => a.Id == id));

        public Task<bool> ExistsAsync(int candidateId, int jobId)
            => Task.FromResult(_applications.Any(a => a.CandidateId == candidateId && a.JobId == jobId));

        public Task InsertAsync(JobCandidateApplication application)
        {
            application.Id = ++_nextId;
            _applications.Add(application);
            return Task.CompletedTask;
        }

        public void Update(JobCandidateApplication application)
        {
            // No-op: the handler mutates the tracked instance in place, which is
            // what EF Core does after a load-then-save cycle.
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}

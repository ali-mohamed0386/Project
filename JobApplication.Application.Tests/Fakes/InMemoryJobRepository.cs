using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Tests.Fakes
{
    /// <summary>
    /// In-memory stand-in for IJobRepository. Entities are stored by reference
    /// and mutated in place, which mirrors how EF Core tracks and persists a
    /// loaded entity after Update + SaveChanges.
    /// </summary>
    internal sealed class InMemoryJobRepository : IJobRepository
    {
        private readonly List<Job> _jobs = new();
        private int _nextId;

        public InMemoryJobRepository(params Job[] seed)
        {
            _jobs.AddRange(seed);
            _nextId = _jobs.Count == 0 ? 0 : _jobs.Max(j => j.Id);
        }

        /// <summary>How many times SaveChangesAsync was called.</summary>
        public int SaveChangesCallCount { get; private set; }

        /// <summary>How many times Update was called.</summary>
        public int UpdateCallCount { get; private set; }

        public IReadOnlyList<Job> Jobs => _jobs;

        public Task InsertAsync(Job job)
        {
            // Mimics the database generating an identity value.
            job.Id = ++_nextId;
            _jobs.Add(job);
            return Task.CompletedTask;
        }

        public void Update(Job job)
        {
            UpdateCallCount++;
        }

        public Task<Job?> GetByIdAsync(int id)
            => Task.FromResult(_jobs.FirstOrDefault(j => j.Id == id));

        public IQueryable<Job> Get() => _jobs.AsQueryable();

        public void Remove(Job job) => _jobs.Remove(job);

        public Task SaveChangesAsync()
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}

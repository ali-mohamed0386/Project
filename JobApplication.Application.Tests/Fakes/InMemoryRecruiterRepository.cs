using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Tests.Fakes
{
    internal sealed class InMemoryRecruiterRepository : IRecruiterRepository
    {
        private readonly List<Recruiter> _recruiters = new();
        private int _nextId;

        public InMemoryRecruiterRepository(params Recruiter[] seed)
        {
            _recruiters.AddRange(seed);
            _nextId = _recruiters.Count == 0 ? 0 : _recruiters.Max(r => r.Id);
        }

        public int SaveChangesCallCount { get; private set; }

        public IReadOnlyList<Recruiter> Recruiters => _recruiters;

        public Task<Recruiter?> GetByIdAsync(int id)
            => Task.FromResult(_recruiters.FirstOrDefault(r => r.Id == id));

        public Task<Recruiter?> GetByEmailAsync(string email)
            => Task.FromResult(_recruiters.FirstOrDefault(r => r.Email == email));

        public Task<bool> EmailExistsAsync(string email)
            => Task.FromResult(_recruiters.Any(r => r.Email == email));

        public Task InsertAsync(Recruiter recruiter)
        {
            recruiter.Id = ++_nextId;
            _recruiters.Add(recruiter);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}

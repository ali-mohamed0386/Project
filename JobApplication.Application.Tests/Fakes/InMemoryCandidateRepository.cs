using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Tests.Fakes
{
    internal sealed class InMemoryCandidateRepository : ICandidateRepository
    {
        private readonly List<Candidate> _candidates = new();
        private int _nextId;

        public InMemoryCandidateRepository(params Candidate[] seed)
        {
            _candidates.AddRange(seed);
            _nextId = _candidates.Count == 0 ? 0 : _candidates.Max(c => c.Id);
        }

        public int SaveChangesCallCount { get; private set; }

        public IReadOnlyList<Candidate> Candidates => _candidates;

        public Task<Candidate?> GetByIdAsync(int id)
            => Task.FromResult(_candidates.FirstOrDefault(c => c.Id == id));

        public Task<Candidate?> GetByEmailAsync(string email)
            => Task.FromResult(_candidates.FirstOrDefault(c => c.Email == email));

        public Task<bool> EmailExistsAsync(string email)
            => Task.FromResult(_candidates.Any(c => c.Email == email));

        public Task InsertAsync(Candidate candidate)
        {
            candidate.Id = ++_nextId;
            _candidates.Add(candidate);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }
}

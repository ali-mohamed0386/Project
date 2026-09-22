using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InsertAsync(Job job)
        {
            await _context.Jobs.AddAsync(job);
        }

        public void Update(Job job)
        {
            _context.Jobs.Update(job);
        }

        public async Task<Job?> GetByIdAsync(int id)
        {
            // Deliberately not using AsNoTracking: the caller (for example
            // CloseAsync) modifies the entity and then calls SaveChanges.
            return await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        }

        public IQueryable<Job> Get()
        {
            var jobs = _context.Jobs.AsQueryable();
            return jobs;
        }

        public void Remove(Job job)
        {
            _context.Jobs.Remove(job);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

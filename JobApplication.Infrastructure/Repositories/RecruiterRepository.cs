using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Repositories
{
    public class RecruiterRepository : IRecruiterRepository
    {
        private readonly ApplicationDbContext _context;

        public RecruiterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Recruiter?> GetByIdAsync(int id)
        {
            return await _context.Recruiters
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Recruiter?> GetByEmailAsync(string email)
        {
            return await _context.Recruiters
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Recruiters
                .AsNoTracking()
                .AnyAsync(r => r.Email == email);
        }

        public async Task InsertAsync(Recruiter recruiter)
        {
            await _context.Recruiters.AddAsync(recruiter);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

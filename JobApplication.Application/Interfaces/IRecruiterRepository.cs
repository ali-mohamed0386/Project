using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface IRecruiterRepository
    {
        Task<Recruiter?> GetByIdAsync(int id);
        Task<Recruiter?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task InsertAsync(Recruiter recruiter);
        Task SaveChangesAsync();
    }
}

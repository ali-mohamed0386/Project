using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface ICandidateRepository
    {
        Task<Candidate?> GetByIdAsync(int id);
        Task<Candidate?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task InsertAsync(Candidate candidate);
        Task SaveChangesAsync();
    }
}

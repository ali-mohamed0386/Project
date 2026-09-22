using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface IJobRepository
    {
        Task InsertAsync(Job job);
        void Update(Job job);
        Task<Job?> GetByIdAsync(int id);
        IQueryable<Job> Get();
        void Remove(Job job);
        Task SaveChangesAsync();
    }
}

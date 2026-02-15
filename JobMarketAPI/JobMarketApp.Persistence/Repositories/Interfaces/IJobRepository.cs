using JobMarketApp.Persistence.Models;

namespace JobMarketApp.Persistence.Repositories.Interfaces
{
    public interface IJobRepository
    {
        Task<List<Job>> GetAllAsync();
        Task<Job> GetByIdAsync(Guid Id);
        Task<Job> CreateAsync(Job job);
        Task<Job> UpdateAsync(Job job);
        Task DeleteAsync(Guid id);
        Task<Job> AcceptAsync(Guid id, Guid contractorId);
    }
}

using JobMarketApp.Persistence.Models;

namespace JobMarketApp.Persistence.Repositories.Interfaces
{
    public interface IContractorRepository
    {
        Task<List<Contractor>> GetAllAsync();
        Task<Contractor> GetByIdAsync(Guid Id);
        Task<Contractor> CreateAsync(Contractor jobOffer);
        Task<Contractor> UpdateAsync(Contractor jobOffer);
        Task DeleteAsync(Guid id);
        Task<List<Contractor?>> SearchAsync(string? term, int page, int pageSize);
    }
}

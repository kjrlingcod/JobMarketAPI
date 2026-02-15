using JobMarketApp.Persistence.Models;

namespace JobMarketApp.Persistence.Repositories.Interfaces
{
    public interface IContractorRepository
    {
        Task<Contractor> GetByIdAsync(Guid Id);
        Task<List<Contractor?>> SearchAsync(string? term, int page, int pageSize);
    }
}

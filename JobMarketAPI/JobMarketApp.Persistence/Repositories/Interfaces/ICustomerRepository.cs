using JobMarketApp.Persistence.Models;

namespace JobMarketApp.Persistence.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer> GetByIdAsync(Guid Id);
        Task<Customer> CreateAsync(Customer jobOffer);
        Task<Customer> UpdateAsync(Customer jobOffer);
        Task DeleteAsync(Guid id);
        Task<List<Customer?>> SearchAsync(string? term, int page, int pageSize);
    }
}

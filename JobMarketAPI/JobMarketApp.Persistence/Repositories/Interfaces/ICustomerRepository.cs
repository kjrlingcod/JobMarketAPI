using JobMarketApp.Persistence.Models;

namespace JobMarketApp.Persistence.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(Guid Id);
        Task<List<Customer?>> SearchAsync(string? term, int page, int pageSize);
    }
}

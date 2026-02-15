
using JobMarketApp.API.DTO.Customers;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<List<CustomerDto>> SearchAsync(string? term, int page, int pageSize);
}
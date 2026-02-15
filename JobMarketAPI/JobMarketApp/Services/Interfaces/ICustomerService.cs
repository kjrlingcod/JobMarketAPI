
using JobMarketApp.API.DTO.Customers;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto customerDto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<CustomerDto?>> SearchAsync(string? term, int page, int pageSize);
}
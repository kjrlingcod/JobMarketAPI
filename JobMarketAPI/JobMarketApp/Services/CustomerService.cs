using AutoMapper;
using JobMarketApp.API.DTO.Customers;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;

namespace JobMarketApp.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IMapper _mapper;
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(IMapper mapper, ICustomerRepository customerRepository)
        {
            _mapper = mapper;
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            var result = await _customerRepository.GetAllAsync();
            return _mapper.Map<List<CustomerDto>>(result);
        }

        public async Task<CustomerDto?> GetByIdAsync(Guid id)
        {
            var result = await _customerRepository.GetByIdAsync(id);

            if (result is null)
                return null;

            return _mapper.Map<CustomerDto>(result);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = _mapper.Map<Customer>(dto);

            var result = await _customerRepository.CreateAsync(customer);

            return _mapper.Map<CustomerDto>(result);
        }

        public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto customerDto)
        {
            // optional safety: ensure exists first
            var existing = await _customerRepository.GetByIdAsync(id);
            if (existing is null)
                return null;

            var customer = _mapper.Map<Customer>(customerDto);

            customer.Id = id;
            var result = await _customerRepository.UpdateAsync(customer);
            return _mapper.Map<CustomerDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _customerRepository.GetByIdAsync(id);
            if (result is null)
                return false;

            await _customerRepository.DeleteAsync(id);
            return true;
        }
        public async Task<List<CustomerDto?>> SearchAsync(string? term, int page, int pageSize)
        {
            var result = await _customerRepository.SearchAsync(term, page, pageSize);
            return _mapper.Map<List<CustomerDto>>(result);
        }
    }
}

using AutoMapper;
using JobMarketApp.API.DTO.Customers;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using static Dapper.SqlMapper;

namespace JobMarketApp.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(IMapper mapper, IMemoryCache cache, ICustomerRepository customerRepository)
        {
            _mapper = mapper;
            _cache = cache;
            _customerRepository = customerRepository;
        }
        public async Task<CustomerDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"customers:{id}";

            if (_cache.TryGetValue(cacheKey, out CustomerDto? cached))
                return cached;

            var result = await _customerRepository.GetByIdAsync(id);

            if (result is null)
                return null;

            var dto = _mapper.Map<CustomerDto>(result);

            // MemoryCaching
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(20),
                Size = 1,
                Priority = CacheItemPriority.High
            };

            _cache.Set(cacheKey, dto, options);

            return dto;
        }
        public async Task<List<CustomerDto>> SearchAsync(string? term, int page, int pageSize)
        {
            var result = await _customerRepository.SearchAsync(term, page, pageSize);
            return _mapper.Map<List<CustomerDto>>(result);
        }
    }
}

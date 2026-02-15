using AutoMapper;
using JobMarketApp.API.DTO.Contractors;
using JobMarketApp.API.DTO.Customers;
using JobMarketApp.Persistence.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace JobMarketApp.API.Services
{
    public class ContractorService : IContractorService
    {
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IContractorRepository _contractorRepository;

        public ContractorService(IMapper mapper, IMemoryCache cache, IContractorRepository contractorRepository)
        {
            _mapper = mapper;
            _cache = cache;
            _contractorRepository = contractorRepository;
        }

        public async Task<ContractorDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"contractors:{id}";

            if (_cache.TryGetValue(cacheKey, out ContractorDto? cached))
                return cached;

            var result = await _contractorRepository.GetByIdAsync(id);

            if (result is null)
                return null;

            var dto = _mapper.Map<ContractorDto>(result);

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
        public async Task<List<ContractorDto>> SearchAsync(string? term, int page, int pageSize)
        {
            var result = await _contractorRepository.SearchAsync(term,page,pageSize);
            return _mapper.Map<List<ContractorDto>>(result);
        }
    }
}
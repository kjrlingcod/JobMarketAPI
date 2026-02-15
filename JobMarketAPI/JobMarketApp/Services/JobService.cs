using AutoMapper;
using JobMarketApp.API.DTO.Jobs;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace JobMarketApp.API.Services
{
    public class JobService : IJobService
    {
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IJobRepository _jobRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IContractorRepository _contractorRepository;

        public JobService(IMapper mapper,
            IMemoryCache cache,
            IJobRepository jobRepository, 
            ICustomerRepository customerRepository,
            IContractorRepository contractorRepository)
        {
            _mapper = mapper;
            _cache = cache;
            _jobRepository = jobRepository;
            _customerRepository = customerRepository;
            _contractorRepository = contractorRepository;
        }

        public async Task<List<JobDto>> GetAllAsync()
        {
            var result = await _jobRepository.GetAllAsync();
            return _mapper.Map<List<JobDto>>(result);
        }

        public async Task<JobDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"jobs:{id}";

            if (_cache.TryGetValue(cacheKey, out JobDto? cached))
                return cached;

            var result = await _jobRepository.GetByIdAsync(id);

            if (result is null)
                return null;

            // MemoryCache
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Size = 1
            });

            return _mapper.Map<JobDto>(result);
        }

        public async Task<JobDto> CreateAsync(CreateJobDto dto)
        {
            if (dto.DueDate < dto.StartDate)
                throw new ArgumentException("DueDate must be on/after StartDate.");

            var existingCustomer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if(existingCustomer == null)
                throw new ArgumentException("Customer does not exist.");

            var job = _mapper.Map<Job>(dto);
            var result = await _jobRepository.CreateAsync(job);

            return _mapper.Map<JobDto>(result);
        }

        public async Task<JobDto?> UpdateAsync(Guid id, UpdateJobDto jobDto)
        {
            if (jobDto.DueDate < jobDto.StartDate)
                throw new ArgumentException("DueDate must be on/after StartDate.");

            var existingCustomer = await _customerRepository.GetByIdAsync(jobDto.CustomerId);
            if (existingCustomer == null)
                throw new ArgumentException("CustomerId does not exist.");

            var existing = await _jobRepository.GetByIdAsync(id);
            if (existing is null)
                return null;

            var job = _mapper.Map<Job>(jobDto);

            job.Id = id;
            var result = await _jobRepository.UpdateAsync(job);
            return _mapper.Map<JobDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _jobRepository.GetByIdAsync(id);
            if (result is null)
                return false;

            await _jobRepository.DeleteAsync(id);
            return true;
        }
    }
}

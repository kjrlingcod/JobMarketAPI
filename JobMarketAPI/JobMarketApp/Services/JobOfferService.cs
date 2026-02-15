using AutoMapper;
using JobMarketApp.API.DTO.JobOffers;
using JobMarketApp.API.DTO.Jobs;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;

namespace JobMarketApp.API.Services
{
    public class JobOfferService : IJobOfferService
    {
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IJobOfferRepository _jobOfferRepository;
        private readonly IContractorRepository _contractorRepository;
        private readonly IJobRepository _jobRepository;

        public JobOfferService(IMapper mapper,
            IMemoryCache cache,
            IJobOfferRepository jobOfferRepository,
            IContractorRepository contractorRepository,
            IJobRepository jobRepository)
        {
            _mapper = mapper;
            _cache = cache;
            _jobOfferRepository = jobOfferRepository;
            _contractorRepository = contractorRepository;
            _jobRepository = jobRepository;
        }

        public async Task<List<JobOfferDto>> GetPaginatedAsync(int page, int pageSize)
        {
            var result = await _jobOfferRepository.GetPaginatedAsync(page, pageSize);
            return _mapper.Map<List<JobOfferDto>>(result);
        }

        public async Task<JobOfferDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"jobOffer:{id}";

            if (_cache.TryGetValue(cacheKey, out JobOfferDto? cached))
                return cached;

            var entity = await _jobOfferRepository.GetByIdAsync(id);
            if (entity is null)
                return null;

            var dto = _mapper.Map<JobOfferDto>(entity);

            //Memorycaching
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(20),
                Size = 1,
                Priority = CacheItemPriority.High
            };

            _cache.Set(cacheKey, dto, options);

            return dto;
        }

        public async Task<JobOfferDto> CreateAsync(CreateJobOfferDto dto)
        {
            var existingJob = await _jobRepository.GetByIdAsync(dto.JobId);
            if (existingJob == null)
                throw new ArgumentException("Job does not exist.");
            else if (dto.Price > existingJob.Budget)
                throw new ValidationException("Price exceeds job budget.");

            var existingContractor = await _contractorRepository.GetByIdAsync(dto.ContractorId);
            if (existingContractor == null)
                throw new ArgumentException("Contractor does not exist.");

            var jobOffer = _mapper.Map<JobOffer>(dto);
            var result = await _jobOfferRepository.CreateAsync(jobOffer);

            return _mapper.Map<JobOfferDto>(result);
        }

        public async Task<JobOfferDto?> UpdateAsync(Guid id, UpdateJobOfferDto jobOfferDto)
        {
            var existingJob = await _jobRepository.GetByIdAsync(jobOfferDto.JobId);
            if (existingJob == null)
                throw new ArgumentException("Job does not exist.");

            var existingContractor = await _contractorRepository.GetByIdAsync(jobOfferDto.ContractorId);
            if (existingContractor == null)
                throw new ArgumentException("Contractor does not exist.");

            var existing = await _jobOfferRepository.GetByIdAsync(id);
            if (existing is null)
                return null;

            var jobOffer = _mapper.Map<JobOffer>(jobOfferDto);

            jobOffer.Id = id;
            var result = await _jobOfferRepository.UpdateAsync(jobOffer);
            return _mapper.Map<JobOfferDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _jobOfferRepository.GetByIdAsync(id);
            if (result is null)
                return false;

            await _jobOfferRepository.DeleteAsync(id);
            return true;
        }
        public async Task<JobDto> AcceptAsync(Guid id, Guid contractorId)
        {
            var existingJob = await _jobRepository.GetByIdAsync(id);
            if (existingJob is null)
                throw new ValidationException("Job does not exist.");
            else if (existingJob?.AcceptedBy != null && existingJob?.AcceptedBy != Guid.Empty)
                throw new ValidationException("Job is no longer available.");

            var existingJobOffer = await _jobOfferRepository.GetByIdAndContractorIdAsync(id, contractorId);
            if(existingJobOffer == null)
                throw new ValidationException("Job offer does not exist.");

            var existingContractor = await _contractorRepository.GetByIdAsync(contractorId);
            if (existingContractor == null)
                throw new ArgumentException("Contractor does not exist.");

            var result = await _jobRepository.AcceptAsync(id, contractorId);
            return _mapper.Map<JobDto>(result);
        }
    }
}
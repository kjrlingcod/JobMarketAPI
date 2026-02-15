using AutoMapper;
using JobMarketApp.API.DTO.Contractors;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories;
using JobMarketApp.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JobMarketApp.API.Services
{
    public class ContractorService : IContractorService
    {
        private readonly IMapper _mapper;
        private readonly IContractorRepository _contractorRepository;

        public ContractorService(IMapper mapper, IContractorRepository contractorRepository)
        {
            _mapper = mapper;
            _contractorRepository = contractorRepository;
        }

        public async Task<List<ContractorDto>> GetAllAsync()
        {
            var result = await _contractorRepository.GetAllAsync();
            return _mapper.Map<List<ContractorDto>>(result);
        }

        public async Task<ContractorDto?> GetByIdAsync(Guid id)
        {
            var result = await _contractorRepository.GetByIdAsync(id);

            if (result is null)
                return null;

            return _mapper.Map<ContractorDto>(result);
        }

        public async Task<ContractorDto> CreateAsync(CreateContractorDto dto)
        {
            var contractor = _mapper.Map<Contractor>(dto);

            var result = await _contractorRepository.CreateAsync(contractor);

            return _mapper.Map<ContractorDto>(result);
        }

        public async Task<ContractorDto?> UpdateAsync(Guid id, UpdateContractorDto contractorDto)
        {
            // optional safety: ensure exists first
            var existing = await _contractorRepository.GetByIdAsync(id);
            if (existing is null)
                return null;

            var contractor = _mapper.Map<Contractor>(contractorDto);

            contractor.Id = id;
            var result = await _contractorRepository.UpdateAsync(contractor);
            return _mapper.Map<ContractorDto>(result);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _contractorRepository.GetByIdAsync(id);
            if (result is null)
                return false;

            await _contractorRepository.DeleteAsync(id);
            return true;
        }
        public async Task<List<ContractorDto?>> SearchAsync(string? term, int page, int pageSize)
        {
            var result = await _contractorRepository.SearchAsync(term,page,pageSize);
            return _mapper.Map<List<ContractorDto>>(result);
        }
    }
}
using JobMarketApp.API.DTO.Contractors;

public interface IContractorService
{
    Task<List<ContractorDto>> GetAllAsync();
    Task<ContractorDto?> GetByIdAsync(Guid id);
    Task<ContractorDto> CreateAsync(CreateContractorDto dto);
    Task<ContractorDto?> UpdateAsync(Guid id, UpdateContractorDto contractorDto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<ContractorDto?>> SearchAsync(string? term, int page, int pageSize);
}
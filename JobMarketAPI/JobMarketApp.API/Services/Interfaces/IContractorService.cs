using JobMarketApp.API.DTO.Contractors;

public interface IContractorService
{
    Task<ContractorDto?> GetByIdAsync(Guid id);
    Task<List<ContractorDto>> SearchAsync(string? term, int page, int pageSize);
}
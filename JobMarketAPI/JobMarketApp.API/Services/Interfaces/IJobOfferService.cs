using JobMarketApp.API.DTO.JobOffers;
using JobMarketApp.API.DTO.Jobs;
using JobMarketApp.Persistence.Models;

public interface IJobOfferService
{
    Task<List<JobOfferDto>> GetPaginatedAsync(int page, int pageSize);
    Task<JobOfferDto?> GetByIdAsync(Guid id);
    Task<JobOfferDto> CreateAsync(CreateJobOfferDto dto);
    Task<JobOfferDto?> UpdateAsync(Guid id, UpdateJobOfferDto jobOfferDto);
    Task<bool> DeleteAsync(Guid id);
    Task<JobDto> AcceptAsync(Guid id, Guid contractorId);

}
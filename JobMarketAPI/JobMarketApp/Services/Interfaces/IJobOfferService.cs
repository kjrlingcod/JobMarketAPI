using JobMarketApp.API.DTO.JobOffers;
using JobMarketApp.API.DTO.Jobs;

public interface IJobOfferService
{
    Task<List<JobOfferDto>> GetAllAsync();
    Task<JobOfferDto?> GetByIdAsync(Guid id);
    Task<JobOfferDto> CreateAsync(CreateJobOfferDto dto);
    Task<JobOfferDto?> UpdateAsync(Guid id, UpdateJobOfferDto jobOfferDto);
    Task<bool> DeleteAsync(Guid id);
    Task<JobDto> AcceptAsync(Guid id, Guid contractorId);

}
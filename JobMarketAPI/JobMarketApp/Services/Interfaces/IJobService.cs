using JobMarketApp.API.DTO.JobOffers;
using JobMarketApp.API.DTO.Jobs;

public interface IJobService
{
    Task<List<JobDto>> GetPaginatedAsync(int page, int pageSize);
    Task<JobDto?> GetByIdAsync(Guid id);
    Task<JobDto> CreateAsync(CreateJobDto dto);
    Task<JobDto?> UpdateAsync(Guid id, UpdateJobDto jobDto);
    Task<bool> DeleteAsync(Guid id);
}
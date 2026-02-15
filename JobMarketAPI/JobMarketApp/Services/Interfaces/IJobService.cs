using JobMarketApp.API.DTO.Jobs;

public interface IJobService
{
    Task<List<JobDto>> GetAllAsync();
    Task<JobDto?> GetByIdAsync(Guid id);
    Task<JobDto> CreateAsync(CreateJobDto dto);
    Task<JobDto?> UpdateAsync(Guid id, UpdateJobDto jobDto);
    Task<bool> DeleteAsync(Guid id);
}
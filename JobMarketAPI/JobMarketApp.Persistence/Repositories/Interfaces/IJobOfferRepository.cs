using JobMarketApp.Persistence.Models;

namespace JobMarketApp.Persistence.Repositories.Interfaces
{
    public interface IJobOfferRepository
    {
        Task<List<JobOffer>> GetAllAsync();
        Task<JobOffer> GetByIdAsync(Guid Id);
        Task<JobOffer> CreateAsync(JobOffer jobOffer);
        Task<JobOffer> UpdateAsync(JobOffer jobOffer);
        Task DeleteAsync(Guid id);
        Task<JobOffer?> GetByIdAndContractorIdAsync(Guid jobId, Guid contractorId);
    }
}

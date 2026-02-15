using Dapper;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;

namespace JobMarketApp.Persistence.Repositories
{
    public class JobOfferRepository : IJobOfferRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public JobOfferRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<JobOffer> CreateAsync(JobOffer jobOffer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO JobOffers (JobId, ContractorId, Price)
                OUTPUT INSERTED.*
                VALUES (@JobId, @ContractorId, @Price);
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@JobId", jobOffer.JobId);
            parameters.Add("@ContractorId", jobOffer.ContractorId);
            parameters.Add("@Price", jobOffer.Price);

            return await connection.QuerySingleAsync<JobOffer>(sql, parameters);
        }

        public async Task<List<JobOffer>> GetAllAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"SELECT * FROM JobOffers
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY;
            ";

            var result = await connection.QueryAsync<JobOffer>(sql);
            return result.ToList();
        }

        public async Task<JobOffer?> GetByIdAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT *
                FROM JobOffers
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QuerySingleOrDefaultAsync<JobOffer>(sql, parameters);
        }

        public async Task<JobOffer?> GetByIdAndContractorIdAsync(Guid jobId, Guid contractorId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT *
                FROM JobOffers
                WHERE JobId = @JobId AND ContractorId = @ContractorId;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@JobId", jobId);
            parameters.Add("@ContractorId", contractorId);

            return await connection.QuerySingleOrDefaultAsync<JobOffer>(sql, parameters);
        }
        public async Task<JobOffer> UpdateAsync(JobOffer jobOffer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                UPDATE JobOffers
                SET Price = @Price
                WHERE Id = @Id;

                SELECT *
                FROM JobOffers
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", jobOffer.Id);
            parameters.Add("@Price", jobOffer.Price);

            return await connection.QuerySingleAsync<JobOffer>(sql, parameters);
        }
        public async Task DeleteAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                DELETE FROM JobOffers
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(sql, parameters);
        }
    }
}

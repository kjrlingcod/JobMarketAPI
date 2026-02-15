using Dapper;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;

namespace JobMarketApp.Persistence.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public JobRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Job> CreateAsync(Job jobOffer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO Jobs (CustomerId, StartDate, DueDate, Budget, Description)
                OUTPUT INSERTED.*
                VALUES (@CustomerId, @StartDate, @DueDate, @Budget, @Description);
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@CustomerId", jobOffer.CustomerId);
            parameters.Add("@StartDate", jobOffer.StartDate);
            parameters.Add("@DueDate", jobOffer.DueDate);
            parameters.Add("@Budget", jobOffer.Budget);
            parameters.Add("@Description", jobOffer.Description);

            return await connection.QuerySingleAsync<Job>(sql, parameters);
        }

        public async Task<List<Job>> GetPaginatedAsync(int page, int pageSize)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT *
                FROM Jobs
                ORDER BY StartDate
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Offset", (page - 1) * pageSize);
            parameters.Add("@PageSize", pageSize);

            var result = await connection.QueryAsync<Job>(sql, parameters);
            return result.ToList();
        }

        public async Task<Job?> GetByIdAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT *
                FROM Jobs
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QuerySingleOrDefaultAsync<Job>(sql, parameters);
        }
        public async Task<Job> UpdateAsync(Job jobOffer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                UPDATE Jobs
                SET StartDate = @StartDate,
                DueDate = @DueDate,
                Budget = @Budget,
                Description = @Description
                WHERE Id = @Id;

                SELECT *
                FROM Jobs
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", jobOffer.Id);
            parameters.Add("@StartDate", jobOffer.StartDate);
            parameters.Add("@DueDate", jobOffer.DueDate);
            parameters.Add("@Budget", jobOffer.Budget);
            parameters.Add("@Description", jobOffer.Description);

            return await connection.QuerySingleAsync<Job>(sql, parameters);
        }
        public async Task DeleteAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                DELETE FROM Jobs
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(sql, parameters);
        }
        public async Task<Job> AcceptAsync(Guid id, Guid contractorId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                UPDATE Jobs
                SET AcceptedBy = @ContractorId
                WHERE Id = @Id;

                SELECT *
                FROM Jobs
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@ContractorId", contractorId);

            return await connection.QuerySingleAsync<Job>(sql, parameters);
        }
    }
}

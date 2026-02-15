using Dapper;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;

namespace JobMarketApp.Persistence.Repositories
{
    public class ContractorRepository : IContractorRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ContractorRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Contractor> CreateAsync(Contractor contractor)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO Contractors (Name, Rating)
                OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Rating
                VALUES (@Name, @Rating);
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Name", contractor.Name);
            parameters.Add("@Rating", contractor.Rating);

            return await connection.QuerySingleAsync<Contractor>(sql, parameters);
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                DELETE FROM Contractors
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<List<Contractor>> GetAllAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"SELECT Id, Name, Rating FROM Contractors";

            var result = await connection.QueryAsync<Contractor>(sql);
            return result.ToList();
        }

        public async Task<Contractor?> GetByIdAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT Id, Name, Rating
                FROM Contractors
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QuerySingleOrDefaultAsync<Contractor>(sql, parameters);
        }

        public async Task<List<Contractor?>> SearchAsync(string? term, int page, int pageSize)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            term = term?.Trim();

            if (string.IsNullOrWhiteSpace(term))
                return new List<Contractor?>();

            var offset = (page - 1) * pageSize;

            string sql;
            var parameters = new DynamicParameters();

            if (Guid.TryParse(term, out var guid))
            {
                sql = @"
                    SELECT *
                    FROM Contractors
                    WHERE Id = @Id
                    ORDER BY Name
                    OFFSET @Offset ROWS 
                    FETCH NEXT @PageSize ROWS ONLY;
                ";

                parameters.Add("@Id", guid);
            }
            else
            {
                sql = @"
                    SELECT *
                    FROM Contractors
                    WHERE Name LIKE @Name
                    ORDER BY Name
                    OFFSET @Offset ROWS 
                    FETCH NEXT @PageSize ROWS ONLY;
                ";

                parameters.Add("@Name","%" + term + "%");
            }

            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", pageSize);

            var result = await connection.QueryAsync<Contractor>(sql, parameters);
            return result.Cast<Contractor?>().ToList();
        }

        public async Task<Contractor> UpdateAsync(Contractor contractor)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                UPDATE Contractors
                SET Name = @Name,
                    Rating = @Rating
                WHERE Id = @Id;

                SELECT Id, Name, Rating
                FROM Contractors
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", contractor.Id);
            parameters.Add("@Name", contractor.Name);
            parameters.Add("@Rating", contractor.Rating);

            return await connection.QuerySingleAsync<Contractor>(sql, parameters);
        }
    }
}
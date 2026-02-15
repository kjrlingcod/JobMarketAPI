using Dapper;
using JobMarketApp.Persistence.Models;
using JobMarketApp.Persistence.Repositories.Interfaces;

namespace JobMarketApp.Persistence.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public CustomerRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO Customers (FirstName, LastName)
                OUTPUT INSERTED.Id, INSERTED.FirstName, INSERTED.LastName
                VALUES (@FirstName, @LastName);
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@FirstName", customer.FirstName);
            parameters.Add("@LastName", customer.LastName);

            return await connection.QuerySingleAsync<Customer>(sql, parameters);
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"SELECT * FROM Customers";

            var result = await connection.QueryAsync<Customer>(sql);
            return result.ToList();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                SELECT *
                FROM Customers
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QuerySingleOrDefaultAsync<Customer>(sql, parameters);
        }

        public async Task<List<Customer?>> SearchAsync(string? term, int page, int pageSize)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            term = term?.Trim();

            if (string.IsNullOrWhiteSpace(term))
                return new List<Customer?>();

            var offset = (page - 1) * pageSize;

            string sql;
            var parameters = new DynamicParameters();

            if (Guid.TryParse(term, out var guid))
            {
                sql = @"
                    SELECT *
                    FROM Customers
                    WHERE Id = @Id
                    OFFSET @Offset ROWS 
                    FETCH NEXT @PageSize ROWS ONLY;
                ";

                parameters.Add("@Id", guid);
            }
            else
            {
                sql = @"
                    SELECT *
                    FROM Customers
                    WHERE FirstName LIKE @FirstName
                    OR LastName LIKE @LastName
                    ORDER BY LastName, FirstName
                    OFFSET @Offset ROWS 
                    FETCH NEXT @PageSize ROWS ONLY;
                ";

                parameters.Add("@FirstName", term + "%");
                parameters.Add("@LastName", term + "%");
            }

            parameters.Add("@Offset", offset);
            parameters.Add("@PageSize", pageSize);

            var result = await connection.QueryAsync<Customer>(sql, parameters);
            return result.Cast<Customer?>().ToList();
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                UPDATE Customers
                SET FirstName = @FirstName,
                    LastName = @LastName
                WHERE Id = @Id;

                SELECT *
                FROM Customers
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", customer.Id);
            parameters.Add("@FirstName", customer.FirstName);
            parameters.Add("@LastName", customer.LastName);

            return await connection.QuerySingleAsync<Customer>(sql, parameters);
        }
        public async Task DeleteAsync(Guid id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                DELETE FROM Customers
                WHERE Id = @Id;
            ";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            await connection.ExecuteAsync(sql, parameters);
        }
    }
}

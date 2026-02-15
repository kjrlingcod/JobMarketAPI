using JobMarketApp.Persistence.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace JobMarketApp.Persistence
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        // Accept the connection string directly
        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace JobMarketApp.API.Database;

public sealed class DbInitializer
{
    private readonly IConfiguration _config;

    public DbInitializer(IConfiguration config)
    {
        _config = config;
    }

    public async Task InitializeAsync()
    {
        var masterConnStr = _config.GetConnectionString("MasterConnection");
        var appConnStr = _config.GetConnectionString("DefaultConnection");
        var dbName = _config["DatabaseName"]; 

        // 1) Create DB if not exists (connect to master)
        await using (var masterConn = new SqlConnection(masterConnStr))
        {
            await masterConn.OpenAsync();

            var createDbSql = $@"
            IF DB_ID(@DbName) IS NULL
            BEGIN
                DECLARE @sql NVARCHAR(MAX) = 'CREATE DATABASE [' + @DbName + ']';
                EXEC(@sql);
            END";
            await masterConn.ExecuteAsync(createDbSql, new { DbName = dbName });
        }

        // 2) Run schema + seed scripts (connect to app DB)
        var createTables = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Database", "001_CreateTables.sql"));
        var seedData = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Database", "002_SeedData.sql"));

        await using (var appConn = new SqlConnection(appConnStr))
        {
            await appConn.OpenAsync();
            await appConn.ExecuteAsync(createTables);
            await appConn.ExecuteAsync(seedData);
        }
    }
}
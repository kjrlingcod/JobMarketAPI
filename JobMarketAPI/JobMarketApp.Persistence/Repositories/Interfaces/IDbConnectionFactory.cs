using System.Data;

namespace JobMarketApp.Persistence.Repositories.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}

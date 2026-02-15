using JobMarketApp.Persistence;
using JobMarketApp.Persistence.Repositories;
using JobMarketApp.Persistence.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobMarketApp.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
        {
            // DB Factory
            services.AddScoped<IDbConnectionFactory>(_ =>
                new DbConnectionFactory(config.GetConnectionString("DefaultConnection")!)
            );

            // Repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IContractorRepository, ContractorRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IJobOfferRepository, JobOfferRepository>();

            return services;
        }
    }
}

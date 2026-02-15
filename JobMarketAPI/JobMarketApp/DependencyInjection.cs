using JobMarketApp.API.Database;
using JobMarketApp.API.Mappings;
using JobMarketApp.API.Services;

namespace JobMarketApp.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers();

            // Scan all Profiles in API assembly
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

            // Services
            services.AddScoped<IContractorService, ContractorService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IJobOfferService, JobOfferService>();

            // Memory Caching
            services.AddMemoryCache(o =>
            {
                o.SizeLimit = 100_000; // “units” you define
            });

            // Add DBInitializer for Creation of DB, Tables and seeding
            services.AddSingleton<DbInitializer>();

            return services;
        }
    }
}

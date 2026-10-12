using FamilyCloud.Application.Repositories;
using FamilyCloud.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyCloud.Infrastructure.DataAccess
{
    public static class PostgreSqlDependecyInjection
    {
        public static IServiceCollection AddPostgreSql(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("familydatabase_test");

            services.AddDbContext<FamilyCloudDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
            services
                .AddScoped<IPhotoRepository, PhotoRepository>()
                .AddScoped<IUnitOfWork, UnitOfWork>();


            return services;
        }
    }
}
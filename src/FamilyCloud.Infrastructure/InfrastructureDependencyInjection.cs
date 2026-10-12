using FamilyCloud.Application.Services.FileStorage;
using FamilyCloud.Infrastructure.DataAccess;
using FamilyCloud.Infrastructure.RabbitMq;
using FamilyCloud.Infrastructure.Services.FileStorage;
using FamilyCloud.Infrastructure.Services.ImageProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyCloud.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileStorageOptions>(
                configuration.GetSection("FileStorage"));
            services.AddRabbitMq(configuration);
            services.AddPostgreSql(configuration);
            services.AddScoped<IImageProcessor, SkiaImageProcessor>()
                .AddScoped<IFileStorage, LocalFileStorage>();

            return services;
        }
    }
}
using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Infrastructure.RabbitMq.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyCloud.Infrastructure.RabbitMq 
{
    public static class RabbitMqDependencyInjection
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

            services.AddSingleton<RabbitMqConnection>();    
            services.AddTransient<IEventPublisher, RabbitMqEventPublisher>();

            return services;
        }
    }
}
using FamilyCloud.Application.Photos.CreateThumbnail;
using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Infrastructure;
using FamilyCloud.Infrastructure.RabbitMq.Publishers;
using FamilyCloud.Infrastructure.Repositories;
using FamilyCloud.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddHostedService<Worker>()
    .AddHostedService<OutboxDispatcher>();

builder.Services
    .AddScoped<OutboxRepository>()
    .AddScoped<CreateThumbnailService>()
    .AddScoped<RabbitMqRetryPublisher>();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

var host = builder.Build();
host.Run();
using FamilyCloud.Infrastructure.RabbitMq;
using FamilyCloud.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));


builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<RabbitMqConnection>();

var host = builder.Build();
host.Run();
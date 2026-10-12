using FamilyCloud.Application.Photos.CreateThumbnail;
using FamilyCloud.Infrastructure;
using FamilyCloud.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<CreateThumbnailService>();
builder.Services.AddInfrastructure(builder.Configuration);


var host = builder.Build();
host.Run();
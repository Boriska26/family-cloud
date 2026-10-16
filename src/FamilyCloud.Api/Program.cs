using FamilyCloud.Application.Photos.Get;
using FamilyCloud.Application.Photos.Upload;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Infrastructure;
using FamilyCloud.Infrastructure.RabbitMq;
using FamilyCloud.Infrastructure.Repositories;
using FamilyCloud.Infrastructure.Services.FileStorage;
using FamilyCloud.Infrastructure.Services.Securities;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddScoped<UploadPhotoService>()
    .AddScoped<GetPhotoService>()
    .AddScoped<IOutboxWriter, OutboxWriter>()
    .AddScoped<IInvitationRepository, InvitationRepository>()
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IPersonRepository, PersonRepository>()
    .AddScoped<CreateInvitationService>();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTransient<RabbitMqTopologyInitializer>();
builder.Services.AddSingleton<RabbitMqConnection>();

builder.Services.AddSingleton<IInvitationTokenService, InvitationTokenService>();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseHttpsRedirection();

using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
{
    var rabbitMqTopologyInitializer = app.Services
        .GetRequiredService<RabbitMqTopologyInitializer>();
    await rabbitMqTopologyInitializer.InitializeAsync(cts.Token);        
}

app.Run();
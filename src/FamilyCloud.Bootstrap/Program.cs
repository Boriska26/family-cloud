using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Bootstrap;
using FamilyCloud.Infrastructure.DataAccess;
using FamilyCloud.Infrastructure.Repositories;
using FamilyCloud.Infrastructure.Services.Securities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<FamilyCloudDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("familydatabase_test")));

builder.Services.AddScoped<CreateFirstAdminService>()
    .AddScoped<Bootstrap>()
    .AddScoped<CreateFirstAdminService>()
    .AddScoped<IPersonRepository, PersonRepository>()
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

Bootstrap bootstrap = scope.ServiceProvider.GetRequiredService<Bootstrap>();

await bootstrap.ExecuteAsync(CancellationToken.None);
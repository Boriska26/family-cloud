using System.Security.Cryptography;
using FamilyCloud.Api.Controllers;
using FamilyCloud.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace FamilyCloud.IntegrationTests;

public sealed class PersonApiFactory : WebApplicationFactory<PersonController>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase($"familycloud_integration_{Guid.NewGuid():N}")
        .WithUsername("integration")
        .WithPassword(Guid.NewGuid().ToString("N"))
        .Build();
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:4-alpine")
        .WithUsername("integration")
        .WithPassword("integration-rabbit-password")
        .Build();
    private readonly string _signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        // These host settings override appsettings and environment variables before startup.
        builder.UseSetting("ConnectionStrings:familydatabase_test", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:SigningKey", _signingKey);
        builder.UseSetting("RabbitMq:Host", _rabbit.Hostname);
        builder.UseSetting("RabbitMq:Port", _rabbit.GetMappedPublicPort(5672).ToString());
        builder.UseSetting("RabbitMq:UserName", "integration");
        builder.UseSetting("RabbitMq:Password", "integration-rabbit-password");
        builder.ConfigureTestServices(services =>
        {
            // Replace every EF options callback so external configuration cannot select a database.
            services.RemoveAll<FamilyCloudDbContext>();
            services.RemoveAll<DbContextOptions<FamilyCloudDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<FamilyCloudDbContext>>();
            services.AddDbContext<FamilyCloudDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbit.StartAsync();
        // Migrate directly with the container connection; never a configured external database.
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public FamilyCloudDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<FamilyCloudDbContext>()
            .UseNpgsql(_postgres.GetConnectionString()).Options);

    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    async Task IAsyncLifetime.DisposeAsync()
    {
        try { await base.DisposeAsync(); }
        finally
        {
            try { await _rabbit.DisposeAsync(); }
            finally { await _postgres.DisposeAsync(); }
        }
    }
}

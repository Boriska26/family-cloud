using System.Security.Claims;
using FamilyCloud.Api.ExceptionHandlers;
using FamilyCloud.Api.Models;
using FamilyCloud.Application;
using FamilyCloud.Application.Exceptions;
using FamilyCloud.Application.Photos.Get;
using FamilyCloud.Application.Photos.Upload;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services;
using FamilyCloud.Application.Services.Securities;
using FamilyCloud.Infrastructure;
using FamilyCloud.Infrastructure.RabbitMq;
using FamilyCloud.Infrastructure.Repositories;
using FamilyCloud.Infrastructure.Services.Securities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOptionsWithValidateOnStart<JwtTokenOptions, JwtTokenOptionsValidator>()
    .Bind(builder.Configuration.GetSection("Jwt"));

builder.Services.AddExceptionHandler<ForbiddenExceptionHandler>()
    .AddExceptionHandler<NotFoundExceptionHandler>()
    .AddExceptionHandler<ConflictExceptionHandler>()
    .AddExceptionHandler<ValidationExceptionHandler>()
    .AddExceptionHandler<InvalidInvitationExceptionHandler>()
    .AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddScoped<UploadPhotoService>()
    .AddScoped<GetPhotoService>()
    .AddScoped<IOutboxWriter, OutboxWriter>()
    .AddScoped<IInvitationRepository, InvitationRepository>()
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IPersonRepository, PersonRepository>()
    .AddScoped<CreateInvitationService>()
    .AddScoped<IPasswordHasherService, PasswordHasherService>()
    .AddScoped<IJwtTokenService, JwtTokenService>()
    .AddScoped<PersonService>()
    .AddScoped<ICurrentUser, CurrentUser>()
    .AddScoped<AcceptInvitationService>()
    .AddScoped<LoginService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        TokenValidationParameters tokenValidationParameters = new TokenValidationParameters();
        string? signingKey = builder.Configuration.GetSection("Jwt").GetValue<string>("SigningKey");
        ArgumentException.ThrowIfNullOrWhiteSpace(signingKey, nameof(signingKey));

        tokenValidationParameters.ValidateIssuer = true;
        tokenValidationParameters.ValidIssuer = JwtTokenOptions.Issuer;

        tokenValidationParameters.ValidateAudience = true;
        tokenValidationParameters.ValidAudience = JwtTokenOptions.Audience;

        tokenValidationParameters.ValidateLifetime = true;

        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(signingKey));
        tokenValidationParameters.ValidateIssuerSigningKey = true;
        tokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.HmacSha256];
        tokenValidationParameters.IssuerSigningKey = securityKey;

        tokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
        tokenValidationParameters.RoleClaimType = ClaimTypes.Role;

        options.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTransient<RabbitMqTopologyInitializer>();
builder.Services.AddSingleton<RabbitMqConnection>();

builder.Services.AddSingleton<IInvitationTokenService, InvitationTokenService>();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Введите JWT токен"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
{
    var rabbitMqTopologyInitializer = app.Services
        .GetRequiredService<RabbitMqTopologyInitializer>();
    await rabbitMqTopologyInitializer.InitializeAsync(cts.Token);
}

app.Run();

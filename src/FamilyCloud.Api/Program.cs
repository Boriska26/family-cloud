using FamilyCloud.Application.Photos.CreateThumbnail;
using FamilyCloud.Application.Photos.Get;
using FamilyCloud.Application.Photos.Upload;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Application.Services.FileStorage;
using FamilyCloud.Infrastructure.DataAccess;
using FamilyCloud.Infrastructure.Repositories;
using FamilyCloud.Infrastructure.Services.FileStorage;
using FamilyCloud.Infrastructure.Services.ImageProcessing;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("familydatabase_test");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<FamilyCloudDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>()
    .AddScoped<IPhotoRepository, PhotoRepository>()
    .AddScoped<UploadPhotoService>()
    .AddScoped<GetPhotoService>()
    .AddScoped<CreateThumbnailService>()
    .AddScoped<IImageProcessor, SkiaImageProcessor>();

builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection("FileStorage"));

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

app.Run();
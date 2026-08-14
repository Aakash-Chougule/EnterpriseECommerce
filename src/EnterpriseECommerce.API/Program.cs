using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Application.Services;
using EnterpriseECommerce.Infrastructure.Persistence;
using EnterpriseECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Add services to the dependency injection container.
// ------------------------------------------------------------

builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// Database Configuration
// ------------------------------------------------------------
// AppDbContext uses Entity Framework Core to communicate
// with PostgreSQL through the Npgsql provider.
// ------------------------------------------------------------

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------------------------------------------------
// Repository Registration
// ------------------------------------------------------------
// The application depends on IProductRepository,
// while Infrastructure provides ProductRepository.
// ------------------------------------------------------------

builder.Services.AddScoped<IProductRepository, ProductRepository>();

// ------------------------------------------------------------
// Business Service Registration
// ------------------------------------------------------------

builder.Services.AddScoped<ProductService>();

var app = builder.Build();

// ------------------------------------------------------------
// HTTP Request Pipeline
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// ------------------------------------------------------------
// Database Seed
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(context);
}

app.Run();
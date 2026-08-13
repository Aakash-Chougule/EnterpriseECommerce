using EnterpriseECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Database Configuration
// ------------------------------------------------------------
// Registers Entity Framework Core with the dependency injection
// container and configures PostgreSQL as the database provider.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------------------------------------------------
// HTTP Request Pipeline
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Enables authorization middleware.
// Authentication will be configured in a later step.
app.UseAuthorization();

// Maps controller endpoints such as:
// GET  /api/products
// POST /api/orders
app.MapControllers();

app.Run();
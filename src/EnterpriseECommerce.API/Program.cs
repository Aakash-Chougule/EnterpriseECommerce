using System.Text;

using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Application.Services;

using EnterpriseECommerce.Infrastructure.Persistence;
using EnterpriseECommerce.Infrastructure.Persistence.Seed;
using EnterpriseECommerce.Infrastructure.Repositories;
using EnterpriseECommerce.Infrastructure.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICE REGISTRATION
// ============================================================

// ------------------------------------------------------------
// MVC / API Controllers
// ------------------------------------------------------------

builder.Services.AddControllers();

// ------------------------------------------------------------
// Cart Repository
// ------------------------------------------------------------

builder.Services.AddScoped<ICartRepository, CartRepository>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();



// ------------------------------------------------------------
// Application Services
// ------------------------------------------------------------

builder.Services.AddScoped<CartService>();

// ------------------------------------------------------------
// Swagger / OpenAPI
// ------------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,

        Description =
            "Enter your JWT access token.\n\n" +
            "Example: Bearer eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document)
            ] = []
        });
});

// ============================================================
// DATABASE CONFIGURATION
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseNpgsql(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"))

        // IMPORTANT:
        // CartRepository.GetByUserIdAsync() loads the Cart
        // and CartItems and the CartService modifies them.
        //
        // Explicitly make sure EF Core uses tracking queries.
        .UseQueryTrackingBehavior(
            QueryTrackingBehavior.TrackAll)

        // Useful while diagnosing EF Core problems.
        .EnableDetailedErrors()

        // Shows entity/key values in EF logs.
        // Remove or disable this in production.
        .EnableSensitiveDataLogging()

        // Log EF Core SQL and change-tracking related activity.
        .LogTo(
            Console.WriteLine,
            LogLevel.Information);
});

// ============================================================
// REPOSITORIES
// ============================================================

// Product Repository

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

// User Repository

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

// Role Repository

builder.Services.AddScoped<
    IRoleRepository,
    RoleRepository>();

// Category Repository

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

// ============================================================
// SECURITY SERVICES
// ============================================================

// Password Hasher

builder.Services.AddScoped<
    IPasswordHasher,
    BCryptPasswordHasher>();

// JWT Token Service

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

// ============================================================
// APPLICATION SERVICES
// ============================================================

// Authentication Service

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Product Service

builder.Services.AddScoped<ProductService>();

// Category Service

builder.Services.AddScoped<CategoryService>();

// ============================================================
// JWT AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings =
            builder.Configuration.GetSection("Jwt");

        var secretKey =
            jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is not configured.");

        var issuer =
            jwtSettings["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is not configured.");

        var audience =
            jwtSettings["Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is not configured.");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // ------------------------------------------------
                // Signature
                // ------------------------------------------------

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),

                // ------------------------------------------------
                // Issuer
                // ------------------------------------------------

                ValidateIssuer = true,

                ValidIssuer = issuer,

                // ------------------------------------------------
                // Audience
                // ------------------------------------------------

                ValidateAudience = true,

                ValidAudience = audience,

                // ------------------------------------------------
                // Lifetime
                // ------------------------------------------------

                ValidateLifetime = true,

                // ------------------------------------------------
                // Clock Skew
                // ------------------------------------------------

                ClockSkew =
                    TimeSpan.FromMinutes(1)
            };

        // ========================================================
        // TEMPORARY JWT DIAGNOSTICS
        // ========================================================

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine();
                Console.WriteLine(
                    "==============================================");

                Console.WriteLine(
                    "JWT AUTHENTICATION FAILED");

                Console.WriteLine(
                    "==============================================");

                Console.WriteLine(
                    $"Exception: {context.Exception.Message}");

                Console.WriteLine(
                    $"Exception Type: " +
                    $"{context.Exception.GetType().Name}");

                Console.WriteLine(
                    "==============================================");

                Console.WriteLine();

                return Task.CompletedTask;
            }
        };
    });

// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();

// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// ------------------------------------------------------------
// HTTPS
// ------------------------------------------------------------

app.UseHttpsRedirection();

// ------------------------------------------------------------
// Authentication
// ------------------------------------------------------------

app.UseAuthentication();

// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

app.UseAuthorization();

// ------------------------------------------------------------
// Controllers
// ------------------------------------------------------------

app.MapControllers();

// ============================================================
// DATABASE INITIALIZATION & SEEDING
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<AppDbContext>();

    // --------------------------------------------------------
    // Apply pending migrations
    // --------------------------------------------------------

    await context.Database.MigrateAsync();

    // --------------------------------------------------------
    // Seed general application data
    // --------------------------------------------------------

    await DbSeeder.SeedAsync(context);

    // --------------------------------------------------------
    // Seed roles
    // --------------------------------------------------------

    await RoleSeeder.SeedAsync(context);

    // --------------------------------------------------------
    // Seed administrator
    // --------------------------------------------------------

    await AdminUserSeeder.SeedAsync(context);
}

// ============================================================
// START APPLICATION
// ============================================================

app.Run();
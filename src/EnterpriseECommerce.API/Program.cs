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
// The following section registers all application services,
// repositories, database services, authentication services,
// and framework services with the ASP.NET Core dependency
// injection container.
// ============================================================


// ------------------------------------------------------------
// MVC / API Controllers
// ------------------------------------------------------------
// Enables controller-based API endpoints such as:
//
// [ApiController]
// [Route("api/[controller]")]
//
// ------------------------------------------------------------

builder.Services.AddControllers();


// ------------------------------------------------------------
// Swagger / OpenAPI
// ------------------------------------------------------------
// Enables Swagger UI for testing and exploring API endpoints
// during development.
// ------------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // --------------------------------------------------------
    // JWT Bearer Authentication for Swagger
    // --------------------------------------------------------

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

    // --------------------------------------------------------
    // Apply JWT Bearer authentication to Swagger
    // --------------------------------------------------------

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
// AppDbContext uses Entity Framework Core to communicate
// with PostgreSQL through the Npgsql provider.
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));


// ============================================================
// REPOSITORIES
// ============================================================
// The Application layer depends on repository interfaces.
// Infrastructure provides their concrete implementations.
//
// Example:
//
// IProductRepository
//        ↓
// ProductRepository
//
// ============================================================


// ------------------------------------------------------------
// Product Repository
// ------------------------------------------------------------

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();


// ------------------------------------------------------------
// User Repository
// ------------------------------------------------------------
// Provides database access for application users.
// ------------------------------------------------------------

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();


// ------------------------------------------------------------
// Role Repository
// ------------------------------------------------------------
// Provides database access for application roles.
// ------------------------------------------------------------

builder.Services.AddScoped<
    IRoleRepository,
    RoleRepository>();


// ============================================================
// SECURITY SERVICES
// ============================================================


// ------------------------------------------------------------
// Password Hasher
// ------------------------------------------------------------
// Application depends on IPasswordHasher.
//
// Infrastructure provides BCryptPasswordHasher.
//
// This keeps BCrypt implementation details outside the
// Application layer.
// ------------------------------------------------------------

builder.Services.AddScoped<
    IPasswordHasher,
    BCryptPasswordHasher>();


// ------------------------------------------------------------
// JWT Token Service
// ------------------------------------------------------------
// Responsible for generating JWT access tokens.
//
// Application depends on:
//
// IJwtTokenService
//
// Infrastructure provides:
//
// JwtTokenService
// ------------------------------------------------------------

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();


// ============================================================
// APPLICATION SERVICES
// ============================================================


// ------------------------------------------------------------
// Authentication Service
// ------------------------------------------------------------
// Handles authentication-related business logic such as:
//
// - User login
// - Password verification
// - JWT generation
// - Authentication response
// ------------------------------------------------------------

builder.Services.AddScoped<
    IAuthService,
    AuthService>();


// ------------------------------------------------------------
// Product Service
// ------------------------------------------------------------
// Handles product-related business operations.
// ------------------------------------------------------------

builder.Services.AddScoped<ProductService>();


// ============================================================
// JWT AUTHENTICATION
// ============================================================
// Configures ASP.NET Core to authenticate users using
// JSON Web Tokens.
//
// Expected HTTP header:
//
// Authorization: Bearer <JWT_TOKEN>
// ============================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // ----------------------------------------------------
        // Read JWT configuration from appsettings.json
        // ----------------------------------------------------

        var jwtSettings =
            builder.Configuration.GetSection("Jwt");


        // ----------------------------------------------------
        // JWT Secret Key
        // ----------------------------------------------------
        // Used to validate the cryptographic signature of
        // incoming JWT tokens.
        // ----------------------------------------------------

        var secretKey =
            jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is not configured.");


        // ----------------------------------------------------
        // JWT Issuer
        // ----------------------------------------------------
        // Identifies the application that issued the token.
        // ----------------------------------------------------

        var issuer =
            jwtSettings["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is not configured.");


        // ----------------------------------------------------
        // JWT Audience
        // ----------------------------------------------------
        // Identifies the intended recipient of the token.
        // ----------------------------------------------------

        var audience =
            jwtSettings["Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is not configured.");


        // ----------------------------------------------------
        // Token Validation Parameters
        // ----------------------------------------------------

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // ------------------------------------------------
                // Signature Validation
                // ------------------------------------------------
                // Ensures that the token was signed using
                // the configured secret key.
                // ------------------------------------------------

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),


                // ------------------------------------------------
                // Issuer Validation
                // ------------------------------------------------
                // Ensures that the token was issued by the
                // expected application.
                // ------------------------------------------------

                ValidateIssuer = true,

                ValidIssuer = issuer,


                // ------------------------------------------------
                // Audience Validation
                // ------------------------------------------------
                // Ensures that the token is intended for
                // this application.
                // ------------------------------------------------

                ValidateAudience = true,

                ValidAudience = audience,


                // ------------------------------------------------
                // Lifetime Validation
                // ------------------------------------------------
                // Rejects expired JWT access tokens.
                // ------------------------------------------------

                ValidateLifetime = true,


                // ------------------------------------------------
                // Clock Skew
                // ------------------------------------------------
                // Allows a small time difference between
                // systems when validating token expiration.
                // ------------------------------------------------

                ClockSkew =
                    TimeSpan.FromMinutes(1)
            };


        // ====================================================
        // TEMPORARY JWT DIAGNOSTICS
        // ====================================================
        // These diagnostics help identify why an incoming
        // JWT token was rejected.
        //
        // This can be reduced or removed later when the
        // authentication system is stable.
        // ====================================================

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
// Enables:
//
// [Authorize]
//
// and role-based authorization:
//
// [Authorize(Roles = "Admin")]
//
// ============================================================

builder.Services.AddAuthorization();


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================


// ------------------------------------------------------------
// Swagger
// ------------------------------------------------------------
// Swagger is enabled only in the Development environment.
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ------------------------------------------------------------
// HTTPS Redirection
// ------------------------------------------------------------
// Redirects HTTP requests to HTTPS.
// ------------------------------------------------------------

app.UseHttpsRedirection();


// ============================================================
// AUTHENTICATION MIDDLEWARE
// ============================================================
// Reads the Authorization header and validates the JWT.
//
// This MUST execute before Authorization.
// ============================================================

app.UseAuthentication();


// ============================================================
// AUTHORIZATION MIDDLEWARE
// ============================================================
// Checks whether the authenticated user has permission
// to access the requested endpoint.
//
// Example:
//
// [Authorize(Roles = "Admin")]
//
// ============================================================

app.UseAuthorization();


// ============================================================
// CONTROLLER ENDPOINTS
// ============================================================
// Maps controller routes such as:
//
// /api/Auth/login
// /api/Products
// /api/Test/admin
//
// ============================================================

app.MapControllers();


// ============================================================
// DATABASE INITIALIZATION & SEEDING
// ============================================================
// The application performs the following operations:
//
// 1. Applies pending EF Core migrations.
// 2. Seeds general application data.
// 3. Seeds authorization roles.
// 4. Seeds the initial administrator account.
//
// The seeders should be idempotent, meaning that running the
// application multiple times should not create duplicate data.
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // --------------------------------------------------------
    // Resolve database context from dependency injection.
    // --------------------------------------------------------

    var context =
        services.GetRequiredService<AppDbContext>();


    // --------------------------------------------------------
    // Apply pending EF Core migrations.
    // --------------------------------------------------------
    // This ensures that the PostgreSQL database structure is
    // synchronized with the application's migrations.
    // --------------------------------------------------------

    await context.Database.MigrateAsync();


    // --------------------------------------------------------
    // Seed general application data.
    // --------------------------------------------------------

    await DbSeeder.SeedAsync(context);


    // --------------------------------------------------------
    // Seed authorization roles.
    // --------------------------------------------------------
    // Roles must exist before users are created because the
    // User entity contains RoleId as a foreign key.
    // --------------------------------------------------------

    await RoleSeeder.SeedAsync(context);


    // --------------------------------------------------------
    // Seed initial administrator account.
    // --------------------------------------------------------
    // AdminUserSeeder requires the Admin role to exist, so
    // this must execute AFTER RoleSeeder.
    // --------------------------------------------------------

    await AdminUserSeeder.SeedAsync(context);
}


// ============================================================
// START APPLICATION
// ============================================================

app.Run();
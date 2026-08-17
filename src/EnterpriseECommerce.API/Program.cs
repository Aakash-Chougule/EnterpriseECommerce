using System.Text;

using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Application.Services;

using EnterpriseECommerce.Infrastructure.Messaging;
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
// Swagger / OpenAPI
// ------------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type =
                SecuritySchemeType.Http,

            Scheme =
                "bearer",

            BearerFormat =
                "JWT",

            In =
                ParameterLocation.Header,

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
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection"))

        // --------------------------------------------------------
        // Keep EF Core entities tracked.
        // --------------------------------------------------------
        .UseQueryTrackingBehavior(
            QueryTrackingBehavior.TrackAll)

        // --------------------------------------------------------
        // Useful during development/debugging.
        // --------------------------------------------------------
        .EnableDetailedErrors()

        // WARNING:
        // Disable this in production because values can appear
        // inside logs.
        .EnableSensitiveDataLogging()

        .LogTo(
            Console.WriteLine,
            LogLevel.Information);
});

// ============================================================
// REPOSITORIES
// ============================================================

builder.Services.AddScoped<
    ICartRepository,
    CartRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IRoleRepository,
    RoleRepository>();

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

// ============================================================
// SECURITY SERVICES
// ============================================================

builder.Services.AddScoped<
    IPasswordHasher,
    BCryptPasswordHasher>();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();

builder.Services.AddScoped<CartService>();

builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<ProductService>();

builder.Services.AddScoped<CategoryService>();

builder.Services.AddScoped<PaymentService>();

// ============================================================
// KAFKA
// ============================================================

builder.Services.AddSingleton<
    IKafkaProducer,
    KafkaProducer>();

// ============================================================
// CORS
// ============================================================
//
// CORS allows our React development application to communicate
// with this ASP.NET Core API.
//
// React is currently running with Vite on localhost:5174.
//
// Later, when the React application is deployed, we will replace
// this URL with the production frontend URL.
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ReactDevelopment",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ============================================================
// JWT AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)

    .AddJwtBearer(options =>
    {
        // --------------------------------------------------------
        // Read JWT configuration
        // --------------------------------------------------------

        var jwtSettings =
            builder.Configuration
                .GetSection("Jwt");

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

        // --------------------------------------------------------
        // Configure JWT validation
        // --------------------------------------------------------

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // Validate token signature.
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            secretKey)),

                // Validate token issuer.
                ValidateIssuer = true,

                ValidIssuer =
                    issuer,

                // Validate token audience.
                ValidateAudience = true,

                ValidAudience =
                    audience,

                // Reject expired tokens.
                ValidateLifetime = true,

                // Small tolerance for clock differences.
                ClockSkew =
                    TimeSpan.FromMinutes(1)
            };

        // ========================================================
        // TEMPORARY JWT DIAGNOSTICS
        // ========================================================

        options.Events =
            new JwtBearerEvents
            {
                OnAuthenticationFailed =
                    context =>
                    {
                        Console.WriteLine();

                        Console.WriteLine(
                            "==============================================");

                        Console.WriteLine(
                            "JWT AUTHENTICATION FAILED");

                        Console.WriteLine(
                            "==============================================");

                        Console.WriteLine(
                            $"Exception: " +
                            $"{context.Exception.Message}");

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
//
// IMPORTANT:
//
// All builder.Services registrations must happen BEFORE this
// line.
//
// After Build(), the service collection becomes read-only.
// ============================================================

var app = builder.Build();

// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

// ------------------------------------------------------------
// Swagger
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// ------------------------------------------------------------
// HTTPS
// ------------------------------------------------------------
//
// Temporarily disabled while React and the API are both being
// developed locally over HTTP.
//
// Later we can configure proper HTTPS development certificates.
// ------------------------------------------------------------

// app.UseHttpsRedirection();

// ------------------------------------------------------------
// CORS
// ------------------------------------------------------------
//
// CORS must run before authentication and authorization so the
// browser is allowed to communicate with the API.
// ------------------------------------------------------------

app.UseCors(
    "ReactDevelopment");

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

using (var scope =
       app.Services.CreateScope())
{
    var services =
        scope.ServiceProvider;

    var context =
        services
            .GetRequiredService<AppDbContext>();

    // --------------------------------------------------------
    // Apply pending migrations
    // --------------------------------------------------------

    await context.Database
        .MigrateAsync();

    // --------------------------------------------------------
    // Seed general application data
    // --------------------------------------------------------

    await DbSeeder
        .SeedAsync(context);

    // --------------------------------------------------------
    // Seed roles
    // --------------------------------------------------------

    await RoleSeeder
        .SeedAsync(context);

    // --------------------------------------------------------
    // Seed administrator
    // --------------------------------------------------------

    await AdminUserSeeder
        .SeedAsync(context);
}

// ============================================================
// START APPLICATION
// ============================================================

app.Run();
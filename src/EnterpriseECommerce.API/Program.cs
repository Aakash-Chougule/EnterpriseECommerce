using System.Text;

using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Application.Security;
using EnterpriseECommerce.Application.Services;

using EnterpriseECommerce.Infrastructure.Messaging;
using EnterpriseECommerce.Infrastructure.Payments;
using EnterpriseECommerce.Infrastructure.Persistence;
using EnterpriseECommerce.Infrastructure.Persistence.Seed;
using EnterpriseECommerce.Infrastructure.Repositories;
using EnterpriseECommerce.Infrastructure.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder =
    WebApplication.CreateBuilder(args);

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

// ============================================================
// SWAGGER / OPENAPI
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name =
                "Authorization",

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

    options.AddSecurityRequirement(
        document =>
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
// DATABASE
// ============================================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options
            .UseNpgsql(
                builder.Configuration
                    .GetConnectionString(
                        "DefaultConnection"))

            .UseQueryTrackingBehavior(
                QueryTrackingBehavior.TrackAll)

            .EnableDetailedErrors()

            // Development only.
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

builder.Services.AddScoped<
    IPermissionRepository,
    PermissionRepository>();

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

builder.Services.AddScoped<
    CartService>();

builder.Services.AddScoped<
    OrderService>();

builder.Services.AddScoped<
    ProductService>();

builder.Services.AddScoped<
    CategoryService>();

builder.Services.AddScoped<
    PaymentService>();

// ============================================================
// PROFILE / ADMIN MANAGEMENT
// ============================================================

builder.Services.AddScoped<
    UserProfileService>();

builder.Services.AddScoped<
    AdminUserService>();

// ============================================================
// RAZORPAY
// ============================================================
//
// RazorpayPaymentService uses HttpClient to communicate
// with the Razorpay REST API.
//
// IMPORTANT:
// Never expose the Razorpay secret key to React.
// The secret must remain on the backend.
// ============================================================

builder.Services.AddHttpClient<
    IRazorpayPaymentService,
    RazorpayPaymentService>();

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
// React development:
// http://localhost:5173
//
// ASP.NET API:
// http://localhost:5042
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
        JwtBearerDefaults
            .AuthenticationScheme)

    .AddJwtBearer(options =>
    {
        var jwtSettings =
            builder.Configuration
                .GetSection(
                    "Jwt");

        var secretKey =
            jwtSettings[
                "SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is not configured.");

        var issuer =
            jwtSettings[
                "Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is not configured.");

        var audience =
            jwtSettings[
                "Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is not configured.");

        // ====================================================
        // TOKEN VALIDATION
        // ====================================================

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey =
                    true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            secretKey)),

                ValidateIssuer =
                    true,

                ValidIssuer =
                    issuer,

                ValidateAudience =
                    true,

                ValidAudience =
                    audience,

                ValidateLifetime =
                    true,

                ClockSkew =
                    TimeSpan.FromMinutes(
                        1)
            };

        // ====================================================
        // DEVELOPMENT JWT ERROR LOGGING
        // ====================================================

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
//
// Permission based authorization.
//
// MAIN ADMIN
// ----------
// Main Admin automatically passes every permission.
//
// NORMAL ADMIN
// ------------
// Normal admins must have the required permission claim.
//
// CUSTOMER
// --------
// Customers cannot access administrative permission policies.
// ============================================================

builder.Services.AddAuthorization(
    options =>
    {
        foreach (
            var permission in
            PermissionNames.All)
        {
            options.AddPolicy(
                permission,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireAssertion(
                        context =>
                        {
                            // ====================================
                            // USER MUST BE ADMIN
                            // ====================================

                            var isAdmin =
                                context.User
                                    .IsInRole(
                                        "Admin");

                            if (!isAdmin)
                            {
                                return false;
                            }

                            // ====================================
                            // MAIN ADMIN
                            // ====================================
                            //
                            // Main Admin always has access.
                            // ====================================

                            var isMainAdmin =
                                context.User
                                    .HasClaim(
                                        "is_main_admin",
                                        "true");

                            if (isMainAdmin)
                            {
                                return true;
                            }

                            // ====================================
                            // NORMAL ADMIN
                            // ====================================
                            //
                            // Check whether the admin's JWT
                            // contains the required permission.
                            // ====================================

                            return context.User
                                .HasClaim(
                                    "permission",
                                    permission);
                        });
                });
        }
    });

// ============================================================
// BUILD APPLICATION
// ============================================================
//
// IMPORTANT:
// All services must be registered BEFORE this line.
// ============================================================

var app =
    builder.Build();

// ============================================================
// SWAGGER
// ============================================================

if (app.Environment
    .IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// ============================================================
// HTTPS
// ============================================================
//
// Currently disabled for local development.
//
// React:
// http://localhost:5173
//
// API:
// http://localhost:5042
//
// app.UseHttpsRedirection();
// ============================================================

// ============================================================
// CORS
// ============================================================
//
// CORS must run before authentication/authorization.
// ============================================================

app.UseCors(
    "ReactDevelopment");

// ============================================================
// AUTHENTICATION
// ============================================================

app.UseAuthentication();

// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// DATABASE MIGRATIONS + SEEDING
// ============================================================
//
// IMPORTANT:
//
// Integration tests create their own application hosts.
//
// Running the production/development seeders from every
// integration test host can result in:
//
// - duplicate permissions
// - duplicate UserPermissions
// - PostgreSQL deadlocks
//
// Therefore normal startup seeding is skipped when the
// environment is IntegrationTesting.
// ============================================================

if (!app.Environment.IsEnvironment(
        "IntegrationTesting"))
{
    using var scope =
        app.Services.CreateScope();

    var services =
        scope.ServiceProvider;

    var context =
        services
            .GetRequiredService<
                AppDbContext>();

    // ========================================================
    // APPLY DATABASE MIGRATIONS
    // ========================================================

    await context.Database
        .MigrateAsync();

    // ========================================================
    // 1. GENERAL APPLICATION DATA
    // ========================================================

    await DbSeeder
        .SeedAsync(
            context);

    // ========================================================
    // 2. ROLES
    // ========================================================
    //
    // Roles must exist before the Main Admin is created.
    //
    // Expected roles:
    //
    // Admin
    // Manager
    // Customer
    // ========================================================

    await RoleSeeder
        .SeedAsync(
            context);

    // ========================================================
    // 3. PERMISSIONS
    // ========================================================
    //
    // Permissions must exist before they can be assigned
    // to administrators.
    //
    // Examples:
    //
    // ManageProducts
    // ManageCategories
    // ManageInventory
    // ManageOrders
    // ManagePayments
    // ManageUsers
    // ManageAdmins
    // ViewReports
    // ========================================================

    await PermissionSeeder
        .SeedAsync(
            context);

    // ========================================================
    // 4. MAIN ADMIN
    // ========================================================
    //
    // This MUST execute after:
    //
    // RoleSeeder
    // PermissionSeeder
    //
    // because the Main Admin requires the Admin role and
    // permission records.
    // ========================================================

    await AdminUserSeeder
        .SeedAsync(
            context);
}

// ============================================================
// RUN APPLICATION
// ============================================================
//
// IMPORTANT:
// There must be only ONE app.Run() in this file.
// ============================================================

app.Run();

// ============================================================
// REQUIRED BY WebApplicationFactory IN INTEGRATION TESTS
// ============================================================
//
// IMPORTANT:
// This must remain at the VERY END of Program.cs.
//
// There must be no top-level statements after this class.
// ============================================================

public partial class Program
{
}
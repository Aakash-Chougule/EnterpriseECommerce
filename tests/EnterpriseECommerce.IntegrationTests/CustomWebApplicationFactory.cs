using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnterpriseECommerce.IntegrationTests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            "Development");

        // ========================================================
        // TEST CONFIGURATION
        // ========================================================

        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            "Host=localhost;" +
            "Port=5432;" +
            "Database=EnterpriseECommerce_IntegrationTests;" +
            "Username=postgres;" +
            "Password=Asd@12345");

        builder.UseSetting(
            "Jwt:SecretKey",
            "EnterpriseECommerce-Integration-Test-JWT-Secret-Key-2026");

        builder.UseSetting(
            "Jwt:Issuer",
            "EnterpriseECommerce.API");

        builder.UseSetting(
            "Jwt:Audience",
            "EnterpriseECommerce.Client");

        builder.UseSetting(
            "Kafka:BootstrapServers",
            "localhost:9092");

        builder.UseSetting(
            "Kafka:OrderEventsTopic",
            "order-events-test");

        builder.UseSetting(
            "Kafka:PaymentEventsTopic",
            "payment-events-test");

        builder.UseSetting(
            "Kafka:OrderStatusEventsTopic",
            "order-status-events-test");

        builder.ConfigureServices(
            services =>
            {
                // ====================================================
                // REPLACE REAL KAFKA
                // ====================================================

                services.RemoveAll<IKafkaProducer>();

                services.AddSingleton<
                    FakeKafkaProducer>();

                services.AddSingleton<
                    IKafkaProducer>(
                        provider =>
                            provider
                                .GetRequiredService<
                                    FakeKafkaProducer>());

                // ====================================================
                // VERIFY TEST DATABASE
                // ====================================================

                var serviceProvider =
                    services.BuildServiceProvider();

                using var scope =
                    serviceProvider
                        .CreateScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<
                            AppDbContext>();

                dbContext.Database
                    .Migrate();
            });
    }
}
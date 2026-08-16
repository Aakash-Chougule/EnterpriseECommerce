using System.Text.Json;

using Confluent.Kafka;

using EnterpriseECommerce.NotificationService.Services;

namespace EnterpriseECommerce.NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var bootstrapServers =
            _configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException(
                "Kafka BootstrapServers is not configured.");

        var groupId =
            _configuration["Kafka:GroupId"]
            ?? throw new InvalidOperationException(
                "Kafka GroupId is not configured.");

        var topic =
            _configuration["Kafka:Topic"]
            ?? throw new InvalidOperationException(
                "Kafka Topic is not configured.");

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer =
            new ConsumerBuilder<Ignore, string>(config)
                .Build();

        consumer.Subscribe(topic);

        _logger.LogInformation(
            "NotificationService subscribed to Kafka topic {Topic}",
            topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result =
                        consumer.Consume(stoppingToken);

                    var message =
                        result.Message.Value;

                    _logger.LogInformation(
                        "Kafka raw message received: {Message}",
                        message);

                    var orderCreatedEvent =
                        JsonSerializer.Deserialize<OrderCreatedEvent>(
                            message,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    if (orderCreatedEvent is null)
                    {
                        _logger.LogWarning(
                            "Kafka message could not be deserialized.");

                        continue;
                    }

                    if (orderCreatedEvent.OrderId == Guid.Empty)
                    {
                        _logger.LogWarning(
                            "Kafka event contains an invalid OrderId.");

                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(
                        orderCreatedEvent.CustomerEmail))
                    {
                        _logger.LogWarning(
                            "Kafka event does not contain a customer email.");

                        continue;
                    }

                    _logger.LogInformation(
                        "OrderCreatedEvent received. " +
                        "OrderId: {OrderId}, " +
                        "OrderNumber: {OrderNumber}, " +
                        "CustomerName: {CustomerName}, " +
                        "CustomerEmail: {CustomerEmail}, " +
                        "TotalAmount: {TotalAmount}, " +
                        "CreatedAt: {CreatedAt}",
                        orderCreatedEvent.OrderId,
                        orderCreatedEvent.OrderNumber,
                        orderCreatedEvent.CustomerName,
                        orderCreatedEvent.CustomerEmail,
                        orderCreatedEvent.TotalAmount,
                        orderCreatedEvent.CreatedAt);

                    // ------------------------------------------------
                    // Send order confirmation email
                    // ------------------------------------------------

                    await _emailService
                        .SendOrderConfirmationAsync(
                            orderCreatedEvent,
                            stoppingToken);

                    // ------------------------------------------------
                    // Commit only after email succeeds.
                    // ------------------------------------------------

                    consumer.Commit(result);

                    _logger.LogInformation(
                        "Kafka event processed successfully for order {OrderNumber}.",
                        orderCreatedEvent.OrderNumber);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Invalid JSON received from Kafka.");
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(
                        ex,
                        "Kafka consume error: {Reason}",
                        ex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process Kafka notification event.");
                }

                await Task.Yield();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Kafka consumer is stopping.");
        }
        finally
        {
            consumer.Close();
        }
    }
}

// ============================================================
// Kafka event contract
// ============================================================

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}
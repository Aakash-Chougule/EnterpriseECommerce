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
        // ========================================================
        // KAFKA CONFIGURATION
        // ========================================================

        var bootstrapServers =
            _configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException(
                "Kafka BootstrapServers is not configured.");

        var groupId =
            _configuration["Kafka:GroupId"]
            ?? throw new InvalidOperationException(
                "Kafka GroupId is not configured.");

        var orderTopic =
            _configuration["Kafka:OrderEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka OrderEventsTopic is not configured.");

        var paymentTopic =
            _configuration["Kafka:PaymentEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka PaymentEventsTopic is not configured.");

        var orderStatusTopic =
            _configuration["Kafka:OrderStatusEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka OrderStatusEventsTopic is not configured.");

        var config =
            new ConsumerConfig
            {
                BootstrapServers =
                    bootstrapServers,

                GroupId =
                    groupId,

                AutoOffsetReset =
                    AutoOffsetReset.Earliest,

                EnableAutoCommit =
                    false
            };

        // ========================================================
        // CREATE CONSUMER
        // ========================================================

        using var consumer =
            new ConsumerBuilder<Ignore, string>(
                config)
                .Build();

        // ========================================================
        // SUBSCRIBE TO ALL NOTIFICATION TOPICS
        // ========================================================

        consumer.Subscribe(
            new[]
            {
                orderTopic,
                paymentTopic,
                orderStatusTopic
            });

        _logger.LogInformation(
            "NotificationService subscribed to Kafka topics: {OrderTopic}, {PaymentTopic}, {OrderStatusTopic}",
            orderTopic,
            paymentTopic,
            orderStatusTopic);

        try
        {
            while (
                !stoppingToken
                    .IsCancellationRequested)
            {
                try
                {
                    var result =
                        consumer.Consume(
                            stoppingToken);

                    var topic =
                        result.Topic;

                    var message =
                        result.Message.Value;

                    _logger.LogInformation(
                        "Kafka message received. Topic: {Topic}, Message: {Message}",
                        topic,
                        message);

                    // ====================================================
                    // ORDER CREATED
                    // ====================================================

                    if (topic == orderTopic)
                    {
                        await ProcessOrderCreatedEventAsync(
                            message,
                            stoppingToken);

                        consumer.Commit(
                            result);

                        continue;
                    }

                    // ====================================================
                    // PAYMENT SUCCEEDED
                    // ====================================================

                    if (topic == paymentTopic)
                    {
                        await ProcessPaymentSucceededEventAsync(
                            message,
                            stoppingToken);

                        consumer.Commit(
                            result);

                        continue;
                    }

                    // ====================================================
                    // ORDER STATUS CHANGED
                    // ====================================================

                    if (topic == orderStatusTopic)
                    {
                        await ProcessOrderStatusChangedEventAsync(
                            message,
                            stoppingToken);

                        consumer.Commit(
                            result);

                        continue;
                    }

                    // ====================================================
                    // UNKNOWN TOPIC
                    // ====================================================

                    _logger.LogWarning(
                        "Received message from unsupported Kafka topic {Topic}.",
                        topic);

                    consumer.Commit(
                        result);
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
                catch (OperationCanceledException)
                {
                    break;
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

    // ============================================================
    // PROCESS ORDER CREATED
    // ============================================================

    private async Task ProcessOrderCreatedEventAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var orderCreatedEvent =
            JsonSerializer.Deserialize<OrderCreatedEvent>(
                message,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive =
                        true
                });

        if (orderCreatedEvent is null)
        {
            throw new JsonException(
                "OrderCreatedEvent could not be deserialized.");
        }

        if (orderCreatedEvent.OrderId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "OrderCreatedEvent contains an invalid OrderId.");
        }

        if (string.IsNullOrWhiteSpace(
            orderCreatedEvent.CustomerEmail))
        {
            throw new InvalidOperationException(
                "OrderCreatedEvent does not contain a customer email.");
        }

        await _emailService
            .SendOrderConfirmationAsync(
                orderCreatedEvent,
                cancellationToken);

        _logger.LogInformation(
            "OrderCreatedEvent processed successfully for order {OrderNumber}.",
            orderCreatedEvent.OrderNumber);
    }

    // ============================================================
    // PROCESS PAYMENT SUCCEEDED
    // ============================================================

    private async Task ProcessPaymentSucceededEventAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var paymentSucceededEvent =
            JsonSerializer.Deserialize<PaymentSucceededEvent>(
                message,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive =
                        true
                });

        if (paymentSucceededEvent is null)
        {
            throw new JsonException(
                "PaymentSucceededEvent could not be deserialized.");
        }

        if (paymentSucceededEvent.PaymentId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "PaymentSucceededEvent contains an invalid PaymentId.");
        }

        if (paymentSucceededEvent.OrderId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "PaymentSucceededEvent contains an invalid OrderId.");
        }

        if (string.IsNullOrWhiteSpace(
            paymentSucceededEvent.CustomerEmail))
        {
            throw new InvalidOperationException(
                "PaymentSucceededEvent does not contain a customer email.");
        }

        await _emailService
            .SendPaymentConfirmationAsync(
                paymentSucceededEvent,
                cancellationToken);

        _logger.LogInformation(
            "PaymentSucceededEvent processed successfully for order {OrderNumber}.",
            paymentSucceededEvent.OrderNumber);
    }

    // ============================================================
    // PROCESS ORDER STATUS CHANGED
    // ============================================================

    private async Task ProcessOrderStatusChangedEventAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var orderStatusEvent =
            JsonSerializer.Deserialize<OrderStatusChangedEvent>(
                message,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive =
                        true
                });

        if (orderStatusEvent is null)
        {
            throw new JsonException(
                "OrderStatusChangedEvent could not be deserialized.");
        }

        if (orderStatusEvent.OrderId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "OrderStatusChangedEvent contains an invalid OrderId.");
        }

        if (string.IsNullOrWhiteSpace(
            orderStatusEvent.CustomerEmail))
        {
            throw new InvalidOperationException(
                "OrderStatusChangedEvent does not contain a customer email.");
        }

        if (string.IsNullOrWhiteSpace(
            orderStatusEvent.NewStatus))
        {
            throw new InvalidOperationException(
                "OrderStatusChangedEvent does not contain a new status.");
        }

        _logger.LogInformation(
            "OrderStatusChangedEvent received. OrderNumber: {OrderNumber}, PreviousStatus: {PreviousStatus}, NewStatus: {NewStatus}",
            orderStatusEvent.OrderNumber,
            orderStatusEvent.PreviousStatus,
            orderStatusEvent.NewStatus);

        await _emailService
            .SendOrderStatusChangedAsync(
                orderStatusEvent,
                cancellationToken);

        _logger.LogInformation(
            "OrderStatusChangedEvent processed successfully for order {OrderNumber}.",
            orderStatusEvent.OrderNumber);
    }
}

// ============================================================
// ORDER CREATED EVENT
// ============================================================

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    public Guid UserId { get; set; }

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}

// ============================================================
// PAYMENT SUCCEEDED EVENT
// ============================================================

public class PaymentSucceededEvent
{
    public Guid PaymentId { get; set; }

    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    public Guid UserId { get; set; }

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } =
        string.Empty;

    public string TransactionId { get; set; } =
        string.Empty;

    public DateTime PaidAt { get; set; }
}

// ============================================================
// ORDER STATUS CHANGED EVENT
// ============================================================

public class OrderStatusChangedEvent
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } =
        string.Empty;

    public Guid UserId { get; set; }

    public string CustomerEmail { get; set; } =
        string.Empty;

    public string CustomerName { get; set; } =
        string.Empty;

    public string PreviousStatus { get; set; } =
        string.Empty;

    public string NewStatus { get; set; } =
        string.Empty;

    public decimal TotalAmount { get; set; }

    public string ShippingAddress { get; set; } =
        string.Empty;

    public DateTime ChangedAt { get; set; }
}
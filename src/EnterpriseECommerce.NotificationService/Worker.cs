using Confluent.Kafka;

namespace EnterpriseECommerce.NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
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

                    _logger.LogInformation(
                        "Kafka message received: {Message}",
                        result.Message.Value);

                    // Later:
                    // Deserialize event
                    // Detect event type
                    // Send email / notification

                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(
                        ex,
                        "Kafka consume error: {Reason}",
                        ex.Error.Reason);
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
using System.Text.Json;

using Confluent.Kafka;

using Microsoft.Extensions.Configuration;

using EnterpriseECommerce.Application.Interfaces;

namespace EnterpriseECommerce.Infrastructure.Messaging;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var bootstrapServers =
            configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException(
                "Kafka BootstrapServers is not configured.");

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer =
            new ProducerBuilder<Null, string>(config)
                .Build();
    }

    public async Task PublishAsync<T>(
        string topic,
        T message,
        CancellationToken cancellationToken = default)
    {
        var json =
            JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(
            topic,
            new Message<Null, string>
            {
                Value = json
            },
            cancellationToken);
    }
}
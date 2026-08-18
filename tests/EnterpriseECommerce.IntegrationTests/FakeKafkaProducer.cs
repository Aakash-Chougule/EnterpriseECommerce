using EnterpriseECommerce.Application.Interfaces;

namespace EnterpriseECommerce.IntegrationTests;

public class FakeKafkaProducer : IKafkaProducer
{
    public List<object> PublishedMessages { get; } =
        new();

    public List<string> PublishedTopics { get; } =
        new();

    public Task PublishAsync<T>(
        string topic,
        T message,
        CancellationToken cancellationToken = default)
    {
        PublishedTopics.Add(topic);

        if (message is not null)
        {
            PublishedMessages.Add(message);
        }

        return Task.CompletedTask;
    }
}
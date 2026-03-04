using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Listener.AntiFraud.Infrastructure.Broker.Publisher;

public class TransactionStatusPublisher : ITransactionStatusPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;

    public TransactionStatusPublisher(IConfiguration configuration)
    {
        _topic = configuration["KAFKA_TOPIC_STATUS"] ?? "transactions.status";

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "kafka:29092",
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync(string transactionExternalId, string status)
    {
        var payload = JsonSerializer.Serialize(new
        {
            transactionExternalId,
            status
        });

        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = payload });
    }

    public void Dispose() => _producer.Dispose();
}

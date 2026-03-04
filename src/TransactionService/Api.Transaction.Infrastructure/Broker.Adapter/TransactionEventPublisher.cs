using System.Text.Json;
using Api.Transaction.Core.Contracts;
using Api.Transaction.Core.Entities;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Api.Transaction.Infrastructure.Broker.Adapter;

/// <summary>
/// Publishes transaction-created events to Kafka.
/// Uses a single long-lived producer instance (thread-safe, should be Scoped or Singleton).
/// </summary>
public class TransactionEventPublisher : ITransactionEventPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;

    public TransactionEventPublisher(IConfiguration configuration)
    {
        _topic = configuration["KAFKA_TOPIC"] ?? "transactions.created";

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "kafka:29092",
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task PublishTransactionCreatedAsync(
        TransactionEntity entity,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            transactionExternalId = entity.TransactionExternalId,
            sourceAccountId = entity.SourceAccountId,
            targetAccountId = entity.TargetAccountId,
            transferTypeId = entity.TransferTypeId,
            value = entity.Value,
            createdAt = entity.CreatedAt
        });

        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = payload }, cancellationToken);
    }

    public void Dispose() => _producer.Dispose();
}

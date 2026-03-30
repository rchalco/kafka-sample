using System.Text.Json;
using Confluent.Kafka;
using Listener.AntiFraud.Core.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Listener.AntiFraud.Infrastructure.Broker.Consumer;

public class TransactionCreatedConsumer : ITransactionCreatedConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger<TransactionCreatedConsumer> _logger;
    private readonly string _topic;

    public TransactionCreatedConsumer(
        IConfiguration configuration,
        ILogger<TransactionCreatedConsumer> logger)
    {
        _logger = logger;
        _topic = configuration["KAFKA_TOPIC"] ?? "transactions.created";

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "kafka:29092",
            GroupId = configuration["KAFKA_CONSUMER_GROUP"] ?? "antifraud-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

    public void Subscribe() => _consumer.Subscribe(_topic);

    /// <summary>
    /// Polls Kafka for the next message and maps it to a <see cref="TransactionCreatedMessage"/>.
    /// Returns null if no message is available within <paramref name="timeout"/>.
    /// </summary>
    public TransactionCreatedMessage? Consume(TimeSpan timeout)
    {
        var result = _consumer.Consume(timeout);
        if (result?.Message?.Value is null) return null;

        try
        {
            using var doc = JsonDocument.Parse(result.Message.Value);
            var root = doc.RootElement;

            var id = root.TryGetProperty("transactionExternalId", out var idProp)
                ? idProp.GetString() ?? "unknown" : "unknown";
            var target = root.TryGetProperty("targetAccountId", out var targetProp)
                ? targetProp.GetString() ?? "" : "";
            var value = root.TryGetProperty("value", out var valueProp)
                ? valueProp.GetDecimal() : 0;
            var createdAt = root.TryGetProperty("createdAt", out var dateProp)
                ? dateProp.GetDateTime() : DateTime.UtcNow;

            return new TransactionCreatedMessage(id, target, value, createdAt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse transaction-created message: {Raw}", result.Message.Value);
            return null;
        }
    }

    public void Close() => _consumer.Close();

    public void Commit() => _consumer.Commit();

    public void Dispose() => _consumer.Dispose();
}


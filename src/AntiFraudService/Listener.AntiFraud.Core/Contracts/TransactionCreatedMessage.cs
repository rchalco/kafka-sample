namespace Listener.AntiFraud.Core.Contracts;

/// <summary>
/// Domain message representing a transaction-created event consumed from Kafka.
/// Decouples the domain from Confluent.Kafka types.
/// </summary>
public record TransactionCreatedMessage(
    string TransactionExternalId,
    string TargetAccountId,
    decimal Value,
    DateTime CreatedAt);

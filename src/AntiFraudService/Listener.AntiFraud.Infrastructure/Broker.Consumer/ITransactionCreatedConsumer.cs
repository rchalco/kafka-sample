using Listener.AntiFraud.Core.Contracts;

namespace Listener.AntiFraud.Infrastructure.Broker.Consumer;

/// <summary>
/// Abstraction over the Kafka consumer for transaction-created events.
/// Returns a domain message to keep Confluent types out of the domain/worker layer.
/// </summary>
public interface ITransactionCreatedConsumer : IDisposable
{
    void Subscribe();
    TransactionCreatedMessage? Consume(TimeSpan timeout);
    void Close();

    public void Commit();
}

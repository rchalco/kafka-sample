namespace Listener.AntiFraud.Infrastructure.Broker.Publisher;

public interface ITransactionStatusPublisher
{
    Task PublishAsync(string transactionExternalId, string status);
}

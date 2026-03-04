using Api.Transaction.Core.Entities;

namespace Api.Transaction.Core.Contracts;

public interface ITransactionEventPublisher
{
    Task PublishTransactionCreatedAsync(TransactionEntity entity, CancellationToken cancellationToken = default);
}

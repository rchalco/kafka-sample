using Api.Transaction.Core.Entities;
using Api.Transaction.Core.Enums;

namespace Api.Transaction.Core.Contracts;

public interface ITransactionRepository
{
    Task AddAsync(TransactionEntity entity, CancellationToken cancellationToken = default);
    Task<TransactionEntity?> GetByExternalIdAsync(Guid transactionExternalId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid transactionExternalId, TransactionStatus status, CancellationToken cancellationToken = default);
}

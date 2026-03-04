using Api.Transaction.Core.Contracts;
using Api.Transaction.Core.Entities;
using Api.Transaction.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Transaction.Infrastructure.Persistence;

public class TransactionRepository(AppDbContext db) : ITransactionRepository
{
    public async Task AddAsync(TransactionEntity entity, CancellationToken cancellationToken = default)
    {
        db.Transactions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TransactionEntity?> GetByExternalIdAsync(
        Guid transactionExternalId,
        CancellationToken cancellationToken = default) =>
        await db.Transactions.FirstOrDefaultAsync(
            t => t.TransactionExternalId == transactionExternalId,
            cancellationToken);

    public async Task UpdateStatusAsync(
        Guid transactionExternalId,
        TransactionStatus status,
        CancellationToken cancellationToken = default)
    {
        var tx = await db.Transactions.FirstOrDefaultAsync(
            t => t.TransactionExternalId == transactionExternalId,
            cancellationToken);

        if (tx is null) return;

        tx.Status = status;
        tx.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}

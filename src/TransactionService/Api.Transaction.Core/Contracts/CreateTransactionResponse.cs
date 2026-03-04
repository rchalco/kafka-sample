namespace Api.Transaction.Core.Contracts;

public record CreateTransactionResponse(
    Guid TransactionExternalId,
    DateTime CreatedAt);

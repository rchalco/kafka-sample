namespace Api.Transaction.Core.Contracts;

public record GetTransactionStatusResponse(
    Guid TransactionExternalId,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

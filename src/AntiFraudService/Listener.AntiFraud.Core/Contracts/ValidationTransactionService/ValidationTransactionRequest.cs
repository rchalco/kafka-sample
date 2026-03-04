namespace Listener.AntiFraud.Core.Contracts.ValidationTransactionService;

public record ValidationTransactionRequest(
    string TransactionExternalId,
    string TargetAccountId,
    decimal Value,
    DateTime CreatedAt);

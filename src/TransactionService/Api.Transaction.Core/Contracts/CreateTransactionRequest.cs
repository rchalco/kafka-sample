namespace Api.Transaction.Core.Contracts;

public record CreateTransactionRequest(
    Guid SourceAccountId,
    Guid TargetAccountId,
    int TranferTypeId,
    decimal Value);

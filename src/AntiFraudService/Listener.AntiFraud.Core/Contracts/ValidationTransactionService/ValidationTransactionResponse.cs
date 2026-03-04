namespace Listener.AntiFraud.Core.Contracts.ValidationTransactionService;

/// <param name="TransactionExternalId">Transaction identifier.</param>
/// <param name="Status">"approved" or "rejected"</param>
public record ValidationTransactionResponse(
    string TransactionExternalId,
    string Status);

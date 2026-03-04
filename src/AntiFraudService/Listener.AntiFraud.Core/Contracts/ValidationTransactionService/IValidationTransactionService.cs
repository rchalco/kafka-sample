namespace Listener.AntiFraud.Core.Contracts.ValidationTransactionService;

public interface IValidationTransactionService
{
    ValidationTransactionResponse Validate(ValidationTransactionRequest request);
}

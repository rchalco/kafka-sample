using System.Collections.Concurrent;
using Listener.AntiFraud.Core.Contracts.ValidationTransactionService;

namespace Listener.AntiFraud.Core.Services;

/// <summary>
/// Domain service that evaluates a transaction against anti-fraud criteria.
/// Registered as Singleton to maintain daily accumulation state in memory.
/// Criteria: value > 2000 OR daily accumulated by target account > 20 000.
/// </summary>
public class ValidationTransactionService : IValidationTransactionService
{
    // key: "targetAccountId:yyyy-MM-dd"
    private readonly ConcurrentDictionary<string, decimal> _dailyAccumulated = new();

    public ValidationTransactionResponse Validate(ValidationTransactionRequest request)
    {
        var dayKey = DateOnly.FromDateTime(request.CreatedAt).ToString("yyyy-MM-dd");
        var accumulationKey = $"{request.TargetAccountId}:{dayKey}";

        var dailyAccumulated = _dailyAccumulated.AddOrUpdate(
            accumulationKey,
            request.Value,
            (_, current) => current + request.Value);

        var status = request.Value > 2000 || dailyAccumulated > 20_000
            ? "rejected"
            : "approved";

        return new ValidationTransactionResponse(request.TransactionExternalId, status);
    }
}

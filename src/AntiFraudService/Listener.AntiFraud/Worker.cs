using Listener.AntiFraud.Core.Contracts.ValidationTransactionService;
using Listener.AntiFraud.Infrastructure.Broker.Consumer;
using Listener.AntiFraud.Infrastructure.Broker.Publisher;

namespace Listener.AntiFraud;

public class Worker(
    ILogger<Worker> logger,
    ITransactionCreatedConsumer consumer,
    IValidationTransactionService validationService,
    ITransactionStatusPublisher publisher) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer.Subscribe();
        logger.LogInformation("Anti-fraud worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = consumer.Consume(TimeSpan.FromSeconds(1));
                if (message is null)
                {
                    await Task.Delay(200, stoppingToken);
                    continue;
                }

                var request = new ValidationTransactionRequest(
                    TransactionExternalId: message.TransactionExternalId,
                    TargetAccountId: message.TargetAccountId,
                    Value: message.Value,
                    CreatedAt: message.CreatedAt);

                var evaluation = validationService.Validate(request);

                logger.LogInformation(
                    "Transaction {TransactionId} evaluated with status {Status}",
                    evaluation.TransactionExternalId,
                    evaluation.Status);

                await publisher.PublishAsync(evaluation.TransactionExternalId, evaluation.Status);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing transaction event");
                await Task.Delay(1000, stoppingToken);
            }
        }

        consumer.Close();
    }
}

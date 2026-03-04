using System.Text.Json;
using Api.Transaction.Core.Contracts;
using Api.Transaction.Core.Enums;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Transaction.Infrastructure.Broker.Adapter;

/// <summary>
/// BackgroundService that consumes transaction status events from Kafka
/// and updates the corresponding transaction in the database.
/// </summary>
public class StatusConsumerService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<StatusConsumerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "kafka:29092";
        var statusTopic = configuration["KAFKA_TOPIC_STATUS"] ?? "transactions.status";
        var groupId = configuration["KAFKA_STATUS_CONSUMER_GROUP"] ?? "transaction-status-group";

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(statusTopic);
        logger.LogInformation("Status consumer listening on topic {Topic}", statusTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result?.Message?.Value is null)
                {
                    await Task.Delay(200, stoppingToken);
                    continue;
                }

                using var doc = JsonDocument.Parse(result.Message.Value);
                var root = doc.RootElement;

                if (!root.TryGetProperty("transactionExternalId", out var idProp)
                    || !Guid.TryParse(idProp.GetString(), out var txId))
                    continue;

                if (!root.TryGetProperty("status", out var statusProp)
                    || !Enum.TryParse<TransactionStatus>(statusProp.GetString(), ignoreCase: true, out var status))
                    continue;

                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                await repository.UpdateStatusAsync(txId, status, stoppingToken);

                logger.LogInformation("Transaction {Id} updated to status {Status}", txId, status);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing status event");
                await Task.Delay(1000, stoppingToken);
            }
        }

        consumer.Close();
    }
}

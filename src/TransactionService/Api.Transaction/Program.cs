using Api.Transaction.Core.Contracts;
using Api.Transaction.Core.Entities;
using Api.Transaction.Core.Enums;
using Api.Transaction.Infrastructure;
using Api.Transaction.Infrastructure.Persistence;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// hacemos correr las migraciones pendientes al iniciar la aplicación
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
     await db.Database.MigrateAsync();
     logger.LogInformation("Database migrations applied successfully.");
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "up" }));

app.MapGet("/health/kafka", (IConfiguration configuration) =>
{
    var bootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:29092";
    try
    {
        using var adminClient = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = bootstrapServers })
            .Build();

        // GetMetadata throws if Kafka is unreachable within the timeout
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
        return Results.Ok(new
        {
            status = "up",
            bootstrapServers,
            brokers = metadata.Brokers.Select(b => new { b.BrokerId, host = b.Host, b.Port })
        });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "down", bootstrapServers, error = ex.Message },
            statusCode: 503);
    }
});

app.MapGet("/hello", async (
    string nameUser,
    CancellationToken cancellationToken) =>
{
    return Results.Ok(new { message = $"Hello, {nameUser}!" });
});


app.MapPost("/transactions", async (
    CreateTransactionRequest request,
    ITransactionRepository repository,
    ITransactionEventPublisher publisher,
    CancellationToken cancellationToken) =>
{
    // Input validation
    if (request.Value <= 0)
        return Results.ValidationProblem(new Dictionary<string, string[]>
            { ["value"] = ["Value must be greater than zero."] });
    if (request.SourceAccountId == Guid.Empty)
        return Results.ValidationProblem(new Dictionary<string, string[]>
            { ["sourceAccountId"] = ["SourceAccountId must not be empty."] });
    if (request.TargetAccountId == Guid.Empty)
        return Results.ValidationProblem(new Dictionary<string, string[]>
            { ["targetAccountId"] = ["TargetAccountId must not be empty."] });
    if (request.TranferTypeId <= 0)
        return Results.ValidationProblem(new Dictionary<string, string[]>
            { ["tranferTypeId"] = ["TransferTypeId must be a positive integer."] });

    var entity = new TransactionEntity
    {
        TransactionExternalId = Guid.NewGuid(),
        SourceAccountId = request.SourceAccountId,
        TargetAccountId = request.TargetAccountId,
        TransferTypeId = request.TranferTypeId,
        Value = request.Value,
        Status = TransactionStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    // 1. Persist with Pending status
    await repository.AddAsync(entity, cancellationToken);

    // 2. Publish to Kafka for anti-fraud evaluation
    await publisher.PublishTransactionCreatedAsync(entity, cancellationToken);

    return Results.Ok(new CreateTransactionResponse(entity.TransactionExternalId, entity.CreatedAt));
});

app.MapGet("/transactions/{id:guid}", async (
    Guid id,
    ITransactionRepository repository,
    CancellationToken cancellationToken) =>
{
    var tx = await repository.GetByExternalIdAsync(id, cancellationToken);
    return tx is null
        ? Results.NotFound()
        : Results.Ok(new GetTransactionStatusResponse(
            tx.TransactionExternalId,
            tx.Status.ToString(),
            tx.CreatedAt,
            tx.UpdatedAt));
});

app.Run();


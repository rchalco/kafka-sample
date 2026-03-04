using Api.Transaction.Core.Contracts;
using Api.Transaction.Infrastructure.Broker.Adapter;
using Api.Transaction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Transaction.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["DB_CONNECTION_STRING"]
            ?? "Host=localhost;Port=5433;Database=yape_db;Username=yape_user;Password=yape_pass";

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly("Api.Transaction.Infrastructure")));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<ITransactionEventPublisher, TransactionEventPublisher>();
        services.AddHostedService<StatusConsumerService>();

        return services;
    }
}

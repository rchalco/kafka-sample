using Listener.AntiFraud;
using Listener.AntiFraud.Core.Contracts.ValidationTransactionService;
using Listener.AntiFraud.Core.Services;
using Listener.AntiFraud.Infrastructure.Broker.Consumer;
using Listener.AntiFraud.Infrastructure.Broker.Publisher;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IValidationTransactionService, ValidationTransactionService>();
builder.Services.AddSingleton<ITransactionCreatedConsumer, TransactionCreatedConsumer>();
builder.Services.AddSingleton<ITransactionStatusPublisher, TransactionStatusPublisher>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

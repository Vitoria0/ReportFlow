using Vendas.Application;
using Vendas.Infrastructure;
using Vendas.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ReportQueueWorker>();

var host = builder.Build();
host.Run();

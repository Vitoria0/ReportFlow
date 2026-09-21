using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vendas.Application.Abstractions;
using Vendas.Infrastructure.Persistence;
using Vendas.Infrastructure.Persistence.Repositories;
using Vendas.Infrastructure.Messaging;

namespace Vendas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VendasDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IVendaRepository, VendaRepository>();
        services.AddScoped<IReportRequestRepository, ReportRequestRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<VendasDbContext>());
        services.AddSingleton<IAmazonSQS>(_ =>
        {
            var region = configuration["AWS:Region"] ?? "us-east-1";
            var serviceUrl = configuration["AWS:SqsServiceUrl"];
            var config = new AmazonSQSConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region),
                ServiceURL = serviceUrl
            };

            return string.IsNullOrWhiteSpace(serviceUrl)
                ? new AmazonSQSClient(config)
                : new AmazonSQSClient(new BasicAWSCredentials("test", "test"), config);
        });
        services.AddScoped<IReportQueuePublisher, SqsReportQueuePublisher>();
        return services;
    }
}

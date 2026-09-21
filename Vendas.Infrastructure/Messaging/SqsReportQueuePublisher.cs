using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Vendas.Application.Abstractions;

namespace Vendas.Infrastructure.Messaging;

public sealed class SqsReportQueuePublisher(
    IAmazonSQS sqsClient,
    IConfiguration configuration) : IReportQueuePublisher
{
    public async Task PublishAsync(
        Guid reportId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var queueName = configuration["AWS:ReportQueueName"] ?? "sales-reports";
        var queueUrl = await GetOrCreateQueueAsync(queueName, cancellationToken);
        var message = new
        {
            ReportId = reportId,
            StartDate = startDate,
            EndDate = endDate
        };

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message)
        }, cancellationToken);
    }

    private async Task<string> GetOrCreateQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        try
        {
            return (await sqsClient.GetQueueUrlAsync(queueName, cancellationToken)).QueueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            return (await sqsClient.CreateQueueAsync(new CreateQueueRequest
            {
                QueueName = queueName
            }, cancellationToken)).QueueUrl;
        }
    }
}

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
        var queueUrl = await EnsureQueueWithDeadLetterQueueAsync(queueName, cancellationToken);
        var message = new
        {
            ReportId = reportId,
            StartDate = startDate,
            EndDate = endDate
        };

        await sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web))
        }, cancellationToken);
    }

    private async Task<string> EnsureQueueWithDeadLetterQueueAsync(
        string queueName,
        CancellationToken cancellationToken)
    {
        var deadLetterQueueName = configuration["AWS:ReportDeadLetterQueueName"] ?? $"{queueName}-dlq";
        var maxReceiveAttempts = GetMaxReceiveAttempts();
        var deadLetterQueueUrl = await GetOrCreateQueueAsync(deadLetterQueueName, cancellationToken);
        var deadLetterQueueArn = await GetQueueArnAsync(deadLetterQueueUrl, cancellationToken);
        var queueUrl = await GetOrCreateQueueAsync(queueName, cancellationToken);

        await sqsClient.SetQueueAttributesAsync(new SetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            Attributes = new Dictionary<string, string>
            {
                [QueueAttributeName.RedrivePolicy] = JsonSerializer.Serialize(new
                {
                    deadLetterTargetArn = deadLetterQueueArn,
                    maxReceiveCount = maxReceiveAttempts.ToString()
                })
            }
        }, cancellationToken);

        return queueUrl;
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

    private async Task<string> GetQueueArnAsync(string queueUrl, CancellationToken cancellationToken)
    {
        var response = await sqsClient.GetQueueAttributesAsync(
            queueUrl,
            new List<string> { QueueAttributeName.QueueArn },
            cancellationToken);
        return response.Attributes[QueueAttributeName.QueueArn];
    }

    private int GetMaxReceiveAttempts() =>
        int.TryParse(configuration["AWS:MaxReceiveAttempts"], out var value) && value > 0
            ? value
            : 3;
}

using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Vendas.Application.Contracts;
using Vendas.Application.UseCases.Relatorios;
using Vendas.Worker.Models;

namespace Vendas.Worker;

public sealed class ReportQueueWorker(
    IAmazonSQS sqsClient,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<ReportQueueWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = configuration["AWS:ReportQueueName"] ?? "sales-reports";
        var queueUrl = await GetOrCreateQueueAsync(queueName, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20,
                VisibilityTimeout = 60
            }, stoppingToken);

            foreach (var message in response.Messages)
                await ProcessMessageAsync(queueUrl, message, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(
        string queueUrl,
        Message message,
        CancellationToken cancellationToken)
    {
        ReportQueueMessage? queueMessage;
        try
        {
            queueMessage = JsonSerializer.Deserialize<ReportQueueMessage>(message.Body, JsonOptions);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Mensagem SQS invalida; removendo mensagem {MessageId}", message.MessageId);
            await DeleteMessageAsync(queueUrl, message, cancellationToken);
            return;
        }

        if (queueMessage is null || queueMessage.ReportId == Guid.Empty)
        {
            logger.LogWarning("Mensagem SQS sem reportId; removendo mensagem {MessageId}", message.MessageId);
            await DeleteMessageAsync(queueUrl, message, cancellationToken);
            return;
        }

        try
        {
            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ProcessarRelatorioHandler>();
            var processed = await handler.HandleAsync(new ProcessarRelatorioCommand(
                queueMessage.ReportId,
                queueMessage.StartDate,
                queueMessage.EndDate), cancellationToken);

            if (!processed)
            {
                logger.LogWarning(
                    "ReportRequest {ReportId} nao encontrada; removendo mensagem {MessageId}",
                    queueMessage.ReportId,
                    message.MessageId);
            }

            await DeleteMessageAsync(queueUrl, message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Falha ao processar ReportRequest {ReportId}; mensagem permanecera para retry",
                queueMessage.ReportId);
        }
    }

    private async Task DeleteMessageAsync(
        string queueUrl,
        Message message,
        CancellationToken cancellationToken)
    {
        await sqsClient.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = queueUrl,
            ReceiptHandle = message.ReceiptHandle
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

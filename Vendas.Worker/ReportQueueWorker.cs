using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Vendas.Application.Contracts;
using Vendas.Application.Abstractions;
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
        var queueUrl = await EnsureQueueWithDeadLetterQueueAsync(queueName, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20,
                VisibilityTimeout = 60,
                MessageSystemAttributeNames = new List<string> { MessageSystemAttributeName.ApproximateReceiveCount }
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
            logger.LogWarning(exception, "Mensagem SQS invalida; permanecera sujeita ao retry {MessageId}", message.MessageId);
            return;
        }

        if (queueMessage is null || queueMessage.ReportId == Guid.Empty)
        {
            logger.LogWarning("Mensagem SQS sem reportId; permanecera sujeita ao retry {MessageId}", message.MessageId);
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

            if (processed is null)
            {
                logger.LogWarning(
                    "ReportRequest {ReportId} nao encontrada; removendo mensagem {MessageId}",
                    queueMessage.ReportId,
                    message.MessageId);
            }
                    else
                    {
                    logger.LogInformation(
                        "Relatorio {ReportId} processado com {QuantidadeVendas} vendas e total {ValorTotal}. Possui vendas: {PossuiVendas}",
                        processed.ReportId,
                        processed.QuantidadeVendas,
                        processed.ValorTotal,
                        processed.PossuiVendas);
                    }

            await DeleteMessageAsync(queueUrl, message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var receiveCount = GetReceiveCount(message);
            var maxReceiveAttempts = GetMaxReceiveAttempts();
            if (receiveCount >= maxReceiveAttempts)
            {
                using var scope = scopeFactory.CreateScope();
                var failureHandler = scope.ServiceProvider.GetRequiredService<IReportRequestFailureHandler>();
                await failureHandler.MarkFailedAsync(
                    queueMessage.ReportId,
                    exception.Message,
                    cancellationToken);
                logger.LogError(
                    exception,
                    "ReportRequest {ReportId} falhou definitivamente na tentativa {ReceiveCount}; mensagem sera encaminhada para a DLQ",
                    queueMessage.ReportId,
                    receiveCount);
            }
            else
            {
                logger.LogError(
                    exception,
                    "Falha temporaria ao processar ReportRequest {ReportId}; tentativa {ReceiveCount} de {MaxReceiveAttempts}, mensagem permanecera para retry",
                    queueMessage.ReportId,
                    receiveCount,
                    maxReceiveAttempts);
            }
        }
    }

    private static int GetReceiveCount(Message message) =>
        message.Attributes.TryGetValue(MessageSystemAttributeName.ApproximateReceiveCount, out var value)
            && int.TryParse(value, out var count)
            ? count
            : 1;

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

    private async Task<string> EnsureQueueWithDeadLetterQueueAsync(
        string queueName,
        CancellationToken cancellationToken)
    {
        var deadLetterQueueName = configuration["AWS:ReportDeadLetterQueueName"] ?? $"{queueName}-dlq";
        var maxReceiveAttempts = configuration.GetValue("AWS:MaxReceiveAttempts", 3);
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

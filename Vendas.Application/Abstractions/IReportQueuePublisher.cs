namespace Vendas.Application.Abstractions;

public interface IReportQueuePublisher
{
    Task PublishAsync(
        Guid reportId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken);
}

namespace Vendas.Application.Abstractions;

public interface IReportRequestFailureHandler
{
    Task MarkFailedAsync(Guid reportId, string errorMessage, CancellationToken cancellationToken);
}

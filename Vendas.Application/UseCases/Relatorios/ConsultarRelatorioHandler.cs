using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;

namespace Vendas.Application.UseCases.Relatorios;

public sealed class ConsultarRelatorioHandler(IReportRequestRepository reportRequestRepository)
{
    public async Task<ConsultarRelatorioResponse?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var reportRequest = await reportRequestRepository.GetByIdAsync(id, cancellationToken);
        if (reportRequest is null)
            return null;

        return new ConsultarRelatorioResponse(
            reportRequest.Id,
            reportRequest.StartDate,
            reportRequest.EndDate,
            reportRequest.Status,
            reportRequest.CreatedAt,
            reportRequest.ProcessedAt,
            reportRequest.ErrorMessage);
    }
}

using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;
using Vendas.Domain.Entities;

namespace Vendas.Application.UseCases.Relatorios;

public sealed class SolicitarRelatorioHandler(
    IReportRequestRepository reportRequestRepository,
    IReportQueuePublisher reportQueuePublisher,
    IUnitOfWork unitOfWork)
{
    public async Task<SolicitarRelatorioResponse> HandleAsync(
        SolicitarRelatorioCommand command,
        CancellationToken cancellationToken)
    {
        var reportRequest = ReportRequest.Create(command.StartDate, command.EndDate);

        await reportRequestRepository.AddAsync(reportRequest, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await reportQueuePublisher.PublishAsync(
            reportRequest.Id,
            reportRequest.StartDate,
            reportRequest.EndDate,
            cancellationToken);

        return new SolicitarRelatorioResponse(reportRequest.Id, reportRequest.Status);
    }
}

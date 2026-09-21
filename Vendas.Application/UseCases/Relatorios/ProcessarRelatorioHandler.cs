using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;

namespace Vendas.Application.UseCases.Relatorios;

public sealed class ProcessarRelatorioHandler(
    IReportRequestRepository reportRequestRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<bool> HandleAsync(
        ProcessarRelatorioCommand command,
        CancellationToken cancellationToken)
    {
        var reportRequest = await reportRequestRepository.GetByIdAsync(command.ReportId, cancellationToken);
        if (reportRequest is null)
            return false;

        reportRequest.MarkProcessing();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        reportRequest.MarkCompleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

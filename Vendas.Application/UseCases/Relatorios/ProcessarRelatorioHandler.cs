using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;

namespace Vendas.Application.UseCases.Relatorios;

public sealed class ProcessarRelatorioHandler(
    IReportRequestRepository reportRequestRepository,
    IVendaRepository vendaRepository,
    IRelatorioWriter relatorioWriter,
    IUnitOfWork unitOfWork)
{
    public async Task<RelatorioProcessadoResponse?> HandleAsync(
        ProcessarRelatorioCommand command,
        CancellationToken cancellationToken)
    {
        var reportRequest = await reportRequestRepository.GetByIdAsync(command.ReportId, cancellationToken);
        if (reportRequest is null)
            return null;

        reportRequest.MarkProcessing();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var vendas = await vendaRepository.ListAsync(
            command.StartDate,
            command.EndDate,
            cancellationToken);
        var quantidadeVendas = vendas.Count;
        var faturamentoTotal = vendas.Sum(venda => venda.ValorTotal);
        var itens = vendas.SelectMany(venda => venda.Itens).ToList();
        var relatorio = new RelatorioVendasResponse(
            reportRequest.Id,
            "Relatorio de Vendas",
            $"{command.StartDate:dd/MM/yyyy} ate {command.EndDate:dd/MM/yyyy}",
            command.StartDate,
            command.EndDate,
            quantidadeVendas,
            itens.Sum(item => item.Quantidade),
            faturamentoTotal,
            quantidadeVendas == 0 ? 0 : faturamentoTotal / quantidadeVendas,
            itens
                .GroupBy(item => item.Produto)
                .Select(group => new DetalhamentoProdutoResponse(
                    group.Key,
                    group.Sum(item => item.Quantidade),
                    group.Sum(item => item.ValorTotal)))
                .OrderBy(item => item.Produto)
                .ToList());

        await relatorioWriter.WriteAsync(relatorio, cancellationToken);
        var resultado = new RelatorioProcessadoResponse(
            reportRequest.Id,
            quantidadeVendas,
            faturamentoTotal,
            quantidadeVendas > 0);

        reportRequest.MarkCompleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return resultado;
    }
}

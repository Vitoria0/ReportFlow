using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;

namespace Vendas.Application.UseCases.Vendas;

public sealed class ListarVendasHandler(IVendaRepository vendaRepository)
{
    public async Task<IReadOnlyCollection<VendaResumoResponse>> HandleAsync(
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
            throw new ArgumentException("A data de inicio deve ser menor ou igual a data de fim.");

        var vendas = await vendaRepository.ListAsync(dataInicio, dataFim, cancellationToken);
        return vendas
            .OrderByDescending(venda => venda.DataVenda)
            .Select(venda => new VendaResumoResponse(
                venda.Id,
                venda.DataVenda,
                venda.Cliente,
                venda.ValorTotal))
            .ToList();
    }
}

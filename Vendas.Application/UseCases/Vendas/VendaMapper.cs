using Vendas.Application.Contracts;
using Vendas.Domain.Entities;

namespace Vendas.Application.UseCases.Vendas;

internal static class VendaMapper
{
    public static VendaResponse ToResponse(Venda venda) => new(
        venda.Id,
        venda.DataVenda,
        venda.Cliente,
        venda.Itens.Select(item => new ItemVendaResponse(
            item.Id,
            item.Produto,
            item.Quantidade,
            item.ValorUnitario,
            item.ValorTotal)).ToList(),
        venda.ValorTotal,
        venda.CreatedAt);
}

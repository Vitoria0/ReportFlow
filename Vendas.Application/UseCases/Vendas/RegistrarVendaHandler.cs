using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;
using Vendas.Domain.Entities;

namespace Vendas.Application.UseCases.Vendas;

public sealed class RegistrarVendaHandler(IVendaRepository vendaRepository, IUnitOfWork unitOfWork)
{
    public async Task<VendaResponse> HandleAsync(
        RegistrarVendaCommand command,
        CancellationToken cancellationToken)
    {
        var itens = command.Itens
            .Select(item => ItemVenda.Create(item.Produto, item.Quantidade, item.ValorUnitario))
            .ToList();
        var venda = Venda.Create(command.DataVenda, command.Cliente, itens);

        await vendaRepository.AddAsync(venda, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return VendaMapper.ToResponse(venda);
    }
}

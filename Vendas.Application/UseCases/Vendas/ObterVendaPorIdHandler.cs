using Vendas.Application.Abstractions;
using Vendas.Application.Contracts;

namespace Vendas.Application.UseCases.Vendas;

public sealed class ObterVendaPorIdHandler(IVendaRepository vendaRepository)
{
    public async Task<VendaResponse?> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var venda = await vendaRepository.GetByIdAsync(id, cancellationToken);
        return venda is null ? null : VendaMapper.ToResponse(venda);
    }
}

using Vendas.Domain.Entities;

namespace Vendas.Application.Abstractions;

public interface IVendaRepository
{
    Task AddAsync(Venda venda, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Venda>> ListAsync(DateTime? dataInicio, DateTime? dataFim, CancellationToken cancellationToken);
    Task<Venda?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

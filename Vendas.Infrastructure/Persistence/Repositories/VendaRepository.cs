using Microsoft.EntityFrameworkCore;
using Vendas.Application.Abstractions;
using Vendas.Domain.Entities;

namespace Vendas.Infrastructure.Persistence.Repositories;

public sealed class VendaRepository(VendasDbContext dbContext) : IVendaRepository
{
    public async Task AddAsync(Venda venda, CancellationToken cancellationToken)
    {
        await dbContext.Vendas.AddAsync(venda, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Venda>> ListAsync(
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Vendas
            .AsNoTracking()
            .Include(venda => venda.Itens)
            .AsQueryable();

        if (dataInicio.HasValue)
            query = query.Where(venda => venda.DataVenda >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(venda => venda.DataVenda <= dataFim.Value);

        return await query
            .OrderByDescending(venda => venda.DataVenda)
            .ToListAsync(cancellationToken);
    }

    public Task<Venda?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Vendas
            .AsNoTracking()
            .Include(venda => venda.Itens)
            .SingleOrDefaultAsync(venda => venda.Id == id, cancellationToken);
}

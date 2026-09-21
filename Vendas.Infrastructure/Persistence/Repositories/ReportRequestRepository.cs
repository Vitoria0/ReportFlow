using Microsoft.EntityFrameworkCore;
using Vendas.Application.Abstractions;
using Vendas.Domain.Entities;

namespace Vendas.Infrastructure.Persistence.Repositories;

public sealed class ReportRequestRepository(VendasDbContext dbContext) : IReportRequestRepository
{
    public async Task AddAsync(ReportRequest reportRequest, CancellationToken cancellationToken)
    {
        await dbContext.ReportRequests.AddAsync(reportRequest, cancellationToken);
    }

    public Task<ReportRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.ReportRequests.SingleOrDefaultAsync(request => request.Id == id, cancellationToken);
}

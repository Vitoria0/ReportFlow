using Vendas.Domain.Entities;

namespace Vendas.Application.Abstractions;

public interface IReportRequestRepository
{
    Task AddAsync(ReportRequest reportRequest, CancellationToken cancellationToken);
    Task<ReportRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

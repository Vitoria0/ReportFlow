using Vendas.Application.Contracts;

namespace Vendas.Application.Abstractions;

public interface IRelatorioWriter
{
    Task WriteAsync(RelatorioVendasResponse relatorio, CancellationToken cancellationToken);
}

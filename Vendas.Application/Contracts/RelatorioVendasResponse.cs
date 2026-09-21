namespace Vendas.Application.Contracts;

public sealed record RelatorioVendasResponse(
    Guid ReportId,
    string Titulo,
    string Periodo,
    DateTime StartDate,
    DateTime EndDate,
    int QuantidadeVendas,
    int QuantidadeItensVendidos,
    decimal FaturamentoTotal,
    decimal TicketMedio,
    IReadOnlyCollection<DetalhamentoProdutoResponse> Detalhamento);

public sealed record DetalhamentoProdutoResponse(
    string Produto,
    int QuantidadeVendida,
    decimal Faturamento);

namespace Vendas.Application.Contracts;

public sealed record RelatorioProcessadoResponse(
    Guid ReportId,
    int QuantidadeVendas,
    decimal ValorTotal,
    bool PossuiVendas);

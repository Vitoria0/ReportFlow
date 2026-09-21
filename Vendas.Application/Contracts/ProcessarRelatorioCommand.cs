namespace Vendas.Application.Contracts;

public sealed record ProcessarRelatorioCommand(
    Guid ReportId,
    DateTime StartDate,
    DateTime EndDate);

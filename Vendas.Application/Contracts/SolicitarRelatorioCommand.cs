namespace Vendas.Application.Contracts;

public sealed record SolicitarRelatorioCommand(DateTime StartDate, DateTime EndDate);

public sealed record SolicitarRelatorioResponse(Guid ReportId, string Status);

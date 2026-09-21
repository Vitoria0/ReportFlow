namespace Vendas.Application.Contracts;

public sealed record ConsultarRelatorioResponse(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    string? ErrorMessage);

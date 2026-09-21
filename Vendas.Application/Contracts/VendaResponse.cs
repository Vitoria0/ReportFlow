namespace Vendas.Application.Contracts;

public sealed record VendaResponse(
    Guid Id,
    DateTime DataVenda,
    string Cliente,
    IReadOnlyCollection<ItemVendaResponse> Itens,
    decimal ValorTotal,
    DateTime CreatedAt);

public sealed record ItemVendaResponse(
    Guid Id,
    string Produto,
    int Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal);

public sealed record VendaResumoResponse(
    Guid Id,
    DateTime DataVenda,
    string Cliente,
    decimal ValorTotal);

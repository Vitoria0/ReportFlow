namespace Vendas.Application.Contracts;

public sealed record RegistrarVendaCommand(
    DateTime DataVenda,
    string Cliente,
    IReadOnlyCollection<RegistrarItemVendaCommand> Itens);

public sealed record RegistrarItemVendaCommand(
    string Produto,
    int Quantidade,
    decimal ValorUnitario);

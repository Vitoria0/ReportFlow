using Vendas.Domain.Exceptions;

namespace Vendas.Domain.Entities;

public sealed class ItemVenda
{
    private ItemVenda()
    {
    }

    private ItemVenda(Guid id, string produto, int quantidade, decimal valorUnitario)
    {
        if (string.IsNullOrWhiteSpace(produto))
            throw new DomainValidationException("O produto e obrigatorio.");

        if (quantidade <= 0)
            throw new DomainValidationException("A quantidade deve ser maior que zero.");

        if (valorUnitario < 0)
            throw new DomainValidationException("O valor unitario nao pode ser negativo.");

        Id = id;
        Produto = produto.Trim();
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorTotal = quantidade * valorUnitario;
    }

    public Guid Id { get; private set; }
    public Guid VendaId { get; private set; }
    public string Produto { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }
    public Venda Venda { get; private set; } = null!;

    public static ItemVenda Create(string produto, int quantidade, decimal valorUnitario) =>
        new(Guid.NewGuid(), produto, quantidade, valorUnitario);
}

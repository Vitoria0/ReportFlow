using Vendas.Domain.Exceptions;

namespace Vendas.Domain.Entities;

public sealed class Venda
{
    private Venda()
    {
    }

    private Venda(Guid id, DateTime dataVenda, string cliente, IReadOnlyCollection<ItemVenda> itens)
    {
        if (string.IsNullOrWhiteSpace(cliente))
            throw new DomainValidationException("O cliente e obrigatorio.");

        if (itens.Count == 0)
            throw new DomainValidationException("A venda deve possuir pelo menos um item.");

        Id = id;
        DataVenda = dataVenda;
        Cliente = cliente.Trim();
        CreatedAt = DateTime.UtcNow;
        Itens = itens.ToList();
        ValorTotal = Itens.Sum(item => item.ValorTotal);
    }

    public Guid Id { get; private set; }
    public DateTime DataVenda { get; private set; }
    public string Cliente { get; private set; } = string.Empty;
    public decimal ValorTotal { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<ItemVenda> Itens { get; private set; } = new List<ItemVenda>();

    public static Venda Create(DateTime dataVenda, string cliente, IReadOnlyCollection<ItemVenda> itens) =>
        new(Guid.NewGuid(), dataVenda, cliente, itens);
}

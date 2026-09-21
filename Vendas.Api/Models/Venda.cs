namespace Vendas.Api.Models;

public sealed class Venda
{
    public Guid Id { get; set; }
    public DateTime DataVenda { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
}

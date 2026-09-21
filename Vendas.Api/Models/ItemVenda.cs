namespace Vendas.Api.Models;

public sealed class ItemVenda
{
    public Guid Id { get; set; }
    public Guid VendaId { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public Venda Venda { get; set; } = null!;
}

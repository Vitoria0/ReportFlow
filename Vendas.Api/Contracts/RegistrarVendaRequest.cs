using System.ComponentModel.DataAnnotations;

namespace Vendas.Api.Contracts;

public sealed class RegistrarVendaRequest
{
    [Required]
    public DateTime? DataVenda { get; init; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Cliente { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<ItemVendaRequest> Itens { get; init; } = [];
}

public sealed class ItemVendaRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Produto { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantidade { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal ValorUnitario { get; init; }
}

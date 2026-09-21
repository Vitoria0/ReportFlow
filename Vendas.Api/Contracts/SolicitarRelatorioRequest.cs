using System.ComponentModel.DataAnnotations;

namespace Vendas.Api.Contracts;

public sealed class SolicitarRelatorioRequest
{
    [Required]
    public DateTime? StartDate { get; init; }

    [Required]
    public DateTime? EndDate { get; init; }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vendas.Api.Contracts;
using Vendas.Api.Data;
using Vendas.Api.Models;

namespace Vendas.Api.Controllers;

[ApiController]
[Route("api/vendas")]
public sealed class VendasController(VendasDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendaResponse>> Registrar(
        RegistrarVendaRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Cliente))
            ModelState.AddModelError(nameof(request.Cliente), "O cliente e obrigatorio.");

        if (request.Itens is null || request.Itens.Count == 0)
            ModelState.AddModelError(nameof(request.Itens), "A venda deve possuir pelo menos um item.");

        if (request.Itens?.Any(item => string.IsNullOrWhiteSpace(item.Produto)) == true)
            ModelState.AddModelError(nameof(request.Itens), "O produto e obrigatorio.");

        if (request.Itens?.Any(item => item.Quantidade <= 0) == true)
            ModelState.AddModelError("Itens", "A quantidade deve ser maior que zero.");

        if (request.Itens?.Any(item => item.ValorUnitario < 0) == true)
            ModelState.AddModelError("Itens", "O valor unitário não pode ser negativo.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var requestedItems = request.Itens!;
        var itens = requestedItems.Select(item => new ItemVenda
        {
            Id = Guid.NewGuid(),
            Produto = item.Produto.Trim(),
            Quantidade = item.Quantidade,
            ValorUnitario = item.ValorUnitario,
            ValorTotal = item.Quantidade * item.ValorUnitario
        }).ToList();

        var venda = new Venda
        {
            Id = Guid.NewGuid(),
            DataVenda = request.DataVenda!.Value,
            Cliente = request.Cliente.Trim(),
            CreatedAt = DateTime.UtcNow,
            Itens = itens,
            ValorTotal = itens.Sum(item => item.ValorTotal)
        };

        dbContext.Vendas.Add(venda);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(venda);
        return CreatedAtAction(nameof(ObterPorId), new { id = venda.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendaResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var venda = await dbContext.Vendas
            .AsNoTracking()
            .Include(item => item.Itens)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return venda is null ? NotFound() : Ok(ToResponse(venda));
    }

    private static VendaResponse ToResponse(Venda venda) => new(
        venda.Id,
        venda.DataVenda,
        venda.Cliente,
        venda.Itens.Select(item => new ItemVendaResponse(
            item.Id,
            item.Produto,
            item.Quantidade,
            item.ValorUnitario,
            item.ValorTotal)).ToList(),
        venda.ValorTotal,
        venda.CreatedAt);
}

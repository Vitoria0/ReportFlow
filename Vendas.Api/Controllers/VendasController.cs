using Microsoft.AspNetCore.Mvc;
using Vendas.Api.Contracts;
using Vendas.Application.Contracts;
using Vendas.Application.UseCases.Vendas;
using Vendas.Domain.Exceptions;

namespace Vendas.Api.Controllers;

[ApiController]
[Route("api/vendas")]
public sealed class VendasController(
    RegistrarVendaHandler registrarVendaHandler,
    ListarVendasHandler listarVendasHandler,
    ObterVendaPorIdHandler obterVendaPorIdHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<VendaResumoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<VendaResumoResponse>>> Listar(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await listarVendasHandler.HandleAsync(dataInicio, dataFim, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendaResponse>> Registrar(
        RegistrarVendaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RegistrarVendaCommand(
                request.DataVenda!.Value,
                request.Cliente,
                request.Itens.Select(item => new RegistrarItemVendaCommand(
                    item.Produto,
                    item.Quantidade,
                    item.ValorUnitario)).ToList());

            var response = await registrarVendaHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
        }
        catch (DomainValidationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendaResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var response = await obterVendaPorIdHandler.HandleAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}

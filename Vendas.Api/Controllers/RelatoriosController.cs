using Microsoft.AspNetCore.Mvc;
using Vendas.Api.Contracts;
using Vendas.Application.Contracts;
using Vendas.Application.UseCases.Relatorios;

namespace Vendas.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
public sealed class RelatoriosController(
    SolicitarRelatorioHandler solicitarRelatorioHandler,
    ConsultarRelatorioHandler consultarRelatorioHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SolicitarRelatorioResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SolicitarRelatorioResponse>> Solicitar(
        SolicitarRelatorioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new SolicitarRelatorioCommand(
                request.StartDate!.Value,
                request.EndDate!.Value);
            var response = await solicitarRelatorioHandler.HandleAsync(command, cancellationToken);

            return Accepted(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("~/reports/{id:guid}")]
    [ProducesResponseType(typeof(ConsultarRelatorioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultarRelatorioResponse>> Consultar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await consultarRelatorioHandler.HandleAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}

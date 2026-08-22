using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Cadastro de pacientes — exclusivo da equipe Cligen (D1). Exclusão sempre lógica (P16).</summary>
[ApiController]
[Route("api/pacientes")]
public sealed class PacientesController(PacienteService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PacienteResumoDto>>> Listar(
        [FromQuery] string? busca, [FromQuery] bool incluirExcluidos, CancellationToken ct)
        => Ok(await service.ListarAsync(busca, incluirExcluidos, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PacienteDto>> Obter(Guid id, CancellationToken ct)
        => await service.ObterAsync(id, ct) is { } p ? Ok(p) : NotFound();

    [HttpPost]
    public async Task<ActionResult<PacienteDto>> Criar([FromBody] SalvarPacienteRequest req, CancellationToken ct)
    {
        var criado = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PacienteDto>> Atualizar(Guid id, [FromBody] SalvarPacienteRequest req, CancellationToken ct)
        => Ok(await service.AtualizarAsync(id, req, ct));

    [HttpPost("{id:guid}/excluir")]
    public async Task<IActionResult> Excluir(Guid id, [FromBody] ExcluirPacienteRequest req, CancellationToken ct)
    {
        await service.ExcluirAsync(id, req, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/restaurar")]
    public async Task<IActionResult> Restaurar(Guid id, CancellationToken ct)
    {
        await service.RestaurarAsync(id, ct);
        return NoContent();
    }
}

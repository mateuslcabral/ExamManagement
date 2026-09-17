using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Cadastro de pacientes. Sem exclusão: o cadastro é único, persistente e parte do prontuário.</summary>
[ApiController]
[Route("api/pacientes")]
public sealed class PacientesController(PacienteService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginaDto<PacienteDto>>> Buscar(
        [FromQuery] string? busca, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        => Ok(await service.BuscarAsync(busca, pagina, tamanhoPagina, ct));

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
}

using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Cligen.Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Exame solicitado (CLG-ESP §6): 1 paciente → N exames. Exclusão sempre lógica (C2). Anexos até 3 (Q22).</summary>
[ApiController]
[Route("api/exames")]
public sealed class ExamesController(ExameService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExameResumoDto>>> Listar(
        [FromQuery] string? busca, [FromQuery] bool incluirExcluidos, CancellationToken ct)
        => Ok(await service.ListarAsync(busca, incluirExcluidos, ct));

    [HttpGet("/api/pacientes/{pacienteId:guid}/exames")]
    public async Task<ActionResult<IReadOnlyList<ExameResumoDto>>> ListarPorPaciente(
        Guid pacienteId, [FromQuery] bool incluirExcluidos, CancellationToken ct)
        => Ok(await service.ListarPorPacienteAsync(pacienteId, incluirExcluidos, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExameDto>> Obter(Guid id, CancellationToken ct)
        => await service.ObterAsync(id, ct) is { } e ? Ok(e) : NotFound();

    [HttpPost]
    public async Task<ActionResult<ExameDto>> Criar([FromBody] CriarExameRequest req, CancellationToken ct)
    {
        var criado = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExameDto>> Atualizar(Guid id, [FromBody] AtualizarExameRequest req, CancellationToken ct)
        => Ok(await service.AtualizarAsync(id, req, ct));

    [HttpPost("{id:guid}/excluir")]
    public async Task<IActionResult> Excluir(Guid id, [FromBody] ExcluirExameRequest req, CancellationToken ct)
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

    // ---- Anexos ----

    [HttpPost("{id:guid}/anexos")]
    [RequestSizeLimit(Anexo.TamanhoMaximoBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = Anexo.TamanhoMaximoBytes + 1024 * 1024)]
    public async Task<ActionResult<AnexoDto>> AdicionarAnexo(Guid id, IFormFile arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Envie um arquivo." });

        await using var conteudo = arquivo.OpenReadStream();
        var anexo = await service.AdicionarAnexoAsync(id, arquivo.FileName, conteudo, arquivo.Length, ct);
        return Created($"/api/exames/{id}/anexos/{anexo.Id}", anexo);
    }

    [HttpGet("{id:guid}/anexos/{anexoId:guid}")]
    public async Task<IActionResult> BaixarAnexo(Guid id, Guid anexoId, CancellationToken ct)
    {
        var arquivo = await service.AbrirAnexoAsync(id, anexoId, ct);
        return File(arquivo.Conteudo, arquivo.TipoConteudo, arquivo.NomeOriginal);
    }

    [HttpDelete("{id:guid}/anexos/{anexoId:guid}")]
    public async Task<IActionResult> RemoverAnexo(Guid id, Guid anexoId, CancellationToken ct)
    {
        await service.RemoverAnexoAsync(id, anexoId, ct);
        return NoContent();
    }
}

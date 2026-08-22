using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Catálogo de exames com CRUD completo (Q3) e parâmetros globais (dias de revisão — P13/P28).</summary>
[ApiController]
[Route("api/catalogo-exames")]
public sealed class ExameCatalogoController(ExameCatalogoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExameCatalogoDto>>> Listar([FromQuery] bool somenteAtivos, CancellationToken ct)
        => Ok(await service.ListarAsync(somenteAtivos, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExameCatalogoDto>> Obter(Guid id, CancellationToken ct)
        => await service.ObterAsync(id, ct) is { } e ? Ok(e) : NotFound();

    [HttpPost]
    public async Task<ActionResult<ExameCatalogoDto>> Criar([FromBody] SalvarExameCatalogoRequest req, CancellationToken ct)
    {
        var criado = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExameCatalogoDto>> Atualizar(Guid id, [FromBody] SalvarExameCatalogoRequest req, CancellationToken ct)
        => Ok(await service.AtualizarAsync(id, req, ct));

    [HttpPost("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid id, CancellationToken ct)
    {
        await service.DesativarAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reativar")]
    public async Task<IActionResult> Reativar(Guid id, CancellationToken ct)
    {
        await service.ReativarAsync(id, ct);
        return NoContent();
    }

    [HttpGet("/api/parametros")]
    public async Task<ActionResult<IReadOnlyList<ParametroDto>>> ListarParametros(CancellationToken ct)
        => Ok(await service.ListarParametrosAsync(ct));

    [HttpPut("/api/parametros/{chave}")]
    public async Task<ActionResult<ParametroDto>> AtualizarParametro(string chave, [FromBody] AtualizarParametroRequest req, CancellationToken ct)
        => Ok(await service.AtualizarParametroAsync(chave, req, ct));
}

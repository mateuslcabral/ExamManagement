using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Catálogo de exames. Sem exclusão: exame aposentado é desativado para preservar o histórico.</summary>
[ApiController]
[Route("api/catalogo-exames")]
public sealed class CatalogoExamesController(ExameCatalogoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExameCatalogoDto>>> Listar([FromQuery] bool apenasAtivos, CancellationToken ct)
        => Ok(await service.ListarAsync(apenasAtivos, ct));

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
}

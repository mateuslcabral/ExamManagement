using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Cligen.Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Base dos cadastros auxiliares (origens, destinos, convênios). Sem exclusão: item aposentado é desativado.</summary>
[ApiController]
public abstract class CadastroControllerBase<T>(CadastroService<T> service) : ControllerBase
    where T : CadastroSimples, ICadastroCriavel<T>
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CadastroDto>>> Listar([FromQuery] bool apenasAtivos, CancellationToken ct)
        => Ok(await service.ListarAsync(apenasAtivos, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CadastroDto>> Obter(Guid id, CancellationToken ct)
        => await service.ObterAsync(id, ct) is { } c ? Ok(c) : NotFound();

    [HttpPost]
    public async Task<ActionResult<CadastroDto>> Criar([FromBody] SalvarCadastroRequest req, CancellationToken ct)
    {
        var criado = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CadastroDto>> Renomear(Guid id, [FromBody] SalvarCadastroRequest req, CancellationToken ct)
        => Ok(await service.RenomearAsync(id, req, ct));

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

[Route("api/cadastros/origens")]
public sealed class OrigensController(CadastroService<Origem> service) : CadastroControllerBase<Origem>(service);

[Route("api/cadastros/destinos")]
public sealed class DestinosController(CadastroService<Destino> service) : CadastroControllerBase<Destino>(service);

[Route("api/cadastros/convenios")]
public sealed class ConveniosController(CadastroService<Convenio> service) : CadastroControllerBase<Convenio>(service);

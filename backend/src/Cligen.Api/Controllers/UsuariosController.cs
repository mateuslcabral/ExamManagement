using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Gestão de Usuários (equipe Cligen). Sem autocadastro: só quem já está autenticado cria contas.</summary>
[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController(UsuarioService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> Listar(CancellationToken ct)
        => Ok(await service.ListarAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> Obter(Guid id, CancellationToken ct)
        => await service.ObterAsync(id, ct) is { } u ? Ok(u) : NotFound();

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Criar([FromBody] CriarUsuarioRequest req, CancellationToken ct)
    {
        var criado = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> Atualizar(Guid id, [FromBody] AtualizarUsuarioRequest req, CancellationToken ct)
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

    [HttpPost("{id:guid}/reenviar-definicao-senha")]
    public async Task<IActionResult> ReenviarDefinicaoSenha(Guid id, CancellationToken ct)
    {
        await service.ReenviarDefinicaoSenhaAsync(id, ct);
        return NoContent();
    }
}

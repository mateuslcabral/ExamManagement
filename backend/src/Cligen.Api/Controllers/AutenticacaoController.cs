using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>
/// Endpoints consumidos pelo BFF (Next.js). A API valida credenciais e devolve identidade;
/// a sessão (cookie) é emitida pelo BFF. Ver docs/02-arquitetura/autenticacao.md.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AutenticacaoController(AutenticacaoService autenticacao, UsuarioService usuarios) : ControllerBase
{
    [HttpPost("validar")]
    public async Task<ActionResult<IdentidadeDto>> Validar([FromBody] ValidarCredenciaisRequest req, CancellationToken ct)
    {
        var identidade = await autenticacao.ValidarCredenciaisAsync(req, ct);
        return identidade is null ? Unauthorized() : Ok(identidade);
    }

    /// <summary>Primeiro acesso de conta local (ou redefinição): o próprio usuário escolhe a senha.</summary>
    [HttpPost("definir-senha")]
    public async Task<IActionResult> DefinirSenha([FromBody] DefinirSenhaRequest req, CancellationToken ct)
    {
        await usuarios.DefinirSenhaAsync(req, ct);
        return NoContent();
    }
}

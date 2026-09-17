using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Api.Configuracao;

namespace Cligen.Api.Middlewares;

/// <summary>
/// Identidade do usuário final, repassada pelo BFF no header X-Usuario-Id. O header só é confiável porque a
/// requisição já passou pela <see cref="ChaveDeServicoMiddleware"/>: apenas o BFF conhece a chave, e ele só
/// preenche o header a partir da própria sessão. Exigido em toda a API, exceto /api/auth (antes do login).
/// Usuário desativado perde o acesso na hora, mesmo com a sessão do BFF ainda válida.
/// </summary>
public sealed class UsuarioAtualMiddleware(RequestDelegate next)
{
    public const string Header = "X-Usuario-Id";

    public async Task InvokeAsync(HttpContext ctx, IUsuarioRepositorio usuarios, UsuarioAtual usuarioAtual)
    {
        if (!ctx.Request.Path.StartsWithSegments("/api") || ctx.Request.Path.StartsWithSegments("/api/auth"))
        {
            await next(ctx);
            return;
        }

        if (!Guid.TryParse(ctx.Request.Headers[Header].ToString(), out var id)
            || await usuarios.ObterPorIdAsync(id, ctx.RequestAborted) is not { Ativo: true })
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { detail = "Usuário ausente, inexistente ou inativo." });
            return;
        }

        usuarioAtual.Definir(id);
        await next(ctx);
    }
}

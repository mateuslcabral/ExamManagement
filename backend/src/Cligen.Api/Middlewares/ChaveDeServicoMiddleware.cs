using System.Security.Cryptography;
using System.Text;

namespace Cligen.Api.Middlewares;

/// <summary>
/// Autenticação servidor-a-servidor: a API só atende o BFF (Next.js), identificado por uma
/// chave de serviço no header X-Api-Key. O navegador nunca chama a API diretamente.
/// Escolha registrada em docs/02-arquitetura/autenticacao.md (fecha a pendência "mecanismo BFF -> API").
/// </summary>
public sealed class ChaveDeServicoMiddleware(RequestDelegate next, IConfiguration config)
{
    private const string Header = "X-Api-Key";
    private readonly byte[] _esperada = Encoding.UTF8.GetBytes(
        config["ChaveDeServico"] ?? throw new InvalidOperationException("ChaveDeServico não configurada."));

    public async Task InvokeAsync(HttpContext ctx)
    {
        // Swagger e health são livres apenas em Development (ver Program.cs).
        if (!ctx.Request.Path.StartsWithSegments("/api"))
        {
            await next(ctx);
            return;
        }

        if (!ctx.Request.Headers.TryGetValue(Header, out var valor)
            || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(valor.ToString()), _esperada))
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsJsonAsync(new { detail = "Chave de serviço ausente ou inválida." });
            return;
        }

        await next(ctx);
    }
}

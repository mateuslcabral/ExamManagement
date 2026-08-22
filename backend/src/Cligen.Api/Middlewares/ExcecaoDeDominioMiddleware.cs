using Cligen.Dominio.Excecoes;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Middlewares;

/// <summary>Traduz exceções de domínio em respostas HTTP (ProblemDetails). Qualquer outra exceção vira 500 genérico.</summary>
public sealed class ExcecaoDeDominioMiddleware(RequestDelegate next, ILogger<ExcecaoDeDominioMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (ExcecaoDeDominio ex)
        {
            var status = ex switch
            {
                EmailJaCadastradoException => StatusCodes.Status409Conflict,
                UsuarioInativoException => StatusCodes.Status403Forbidden,
                OperacaoInvalidaParaTipoLoginException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            await Escrever(ctx, status, ex.Message);
        }
        catch (Exception ex) when (!ctx.Response.HasStarted)
        {
            logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}", ctx.Request.Method, ctx.Request.Path);
            await Escrever(ctx, StatusCodes.Status500InternalServerError, "Erro interno.");
        }
    }

    private static Task Escrever(HttpContext ctx, int status, string detalhe)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        return ctx.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Detail = detalhe });
    }
}

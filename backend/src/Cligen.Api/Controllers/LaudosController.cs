using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Acolhimento de amostra (§7) e fluxo do laudo (§8): etapas, substituição e disponibilização manual.</summary>
[ApiController]
[Route("api/exames/{exameId:guid}")]
public sealed class LaudosController(AcolhimentoService acolhimento, LaudoService laudos) : ControllerBase
{
    // ---- Amostra ----

    [HttpPost("amostras")]
    public async Task<ActionResult<ExameDto>> Acolher(Guid exameId, [FromBody] AcolherAmostraRequest req, CancellationToken ct)
        => Ok(await acolhimento.AcolherAsync(exameId, req, ct));

    [HttpPost("amostras/rejeitar")]
    public async Task<ActionResult<ExameDto>> Rejeitar(Guid exameId, [FromBody] RejeitarAmostraRequest req, CancellationToken ct)
        => Ok(await acolhimento.RejeitarAsync(exameId, req, ct));

    // ---- Laudo ----

    /// <summary>Registra a etapa (avança o estado) ou substitui o arquivo dela. Campo multipart: <c>arquivo</c> (PDF) e <c>usuarioId</c>.</summary>
    [HttpPost("laudo/{etapa}")]
    [RequestSizeLimit(EtapaAndamento.TamanhoMaximoBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = EtapaAndamento.TamanhoMaximoBytes + 1024 * 1024)]
    public async Task<ActionResult<ExameDto>> EnviarEtapa(Guid exameId, TipoEtapaLaudo etapa, IFormFile arquivo, [FromForm] Guid usuarioId, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            return BadRequest(new ProblemDetails { Status = 400, Detail = "Envie um arquivo PDF." });

        await using var conteudo = arquivo.OpenReadStream();
        return Ok(await laudos.EnviarArquivoEtapaAsync(exameId, etapa, arquivo.FileName, conteudo, arquivo.Length, usuarioId, ct));
    }

    [HttpGet("laudo/{etapa}/arquivo")]
    public async Task<IActionResult> BaixarEtapa(Guid exameId, TipoEtapaLaudo etapa, CancellationToken ct)
    {
        var arquivo = await laudos.AbrirArquivoEtapaAsync(exameId, etapa, ct);
        return File(arquivo.Conteudo, arquivo.TipoConteudo, arquivo.NomeOriginal);
    }

    [HttpPost("laudo/disponibilizar")]
    public async Task<ActionResult<ExameDto>> Disponibilizar(Guid exameId, CancellationToken ct)
        => Ok(await laudos.DisponibilizarAsync(exameId, ct));
}

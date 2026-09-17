using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Cligen.Dominio.Comum;
using Cligen.Dominio.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Fluxo do laudo: etapas com PDF (substituível), download e disponibilização manual ao paciente.</summary>
[ApiController]
[Route("api/exames/{exameId:guid}/laudo")]
public sealed class LaudosController(LaudoService service) : ControllerBase
{
    private const long LimiteUploadBytes = FormatoArquivo.TamanhoMaximoBytes + 1024 * 1024;

    /// <summary>Registra a etapa (avança o estado) ou substitui o arquivo dela. Campo multipart: <c>arquivo</c> (PDF).</summary>
    [HttpPost("{etapa}")]
    [RequestSizeLimit(LimiteUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteUploadBytes)]
    public async Task<ActionResult<ExameDto>> EnviarEtapa(Guid exameId, TipoEtapaLaudo etapa, IFormFile arquivo, CancellationToken ct)
    {
        await using var conteudo = arquivo.OpenReadStream();
        return Ok(await service.EnviarArquivoEtapaAsync(exameId, etapa, conteudo, arquivo.FileName, arquivo.Length, ct));
    }

    [HttpGet("{etapa}/arquivo")]
    public async Task<IActionResult> BaixarEtapa(Guid exameId, TipoEtapaLaudo etapa, CancellationToken ct)
        => await service.AbrirArquivoEtapaAsync(exameId, etapa, ct) is { } r
            ? File(r.Conteudo, "application/pdf", r.Etapa.NomeOriginal)
            : NotFound();

    [HttpPost("disponibilizar")]
    public async Task<ActionResult<ExameDto>> Disponibilizar(Guid exameId, CancellationToken ct)
        => Ok(await service.DisponibilizarAsync(exameId, ct));
}

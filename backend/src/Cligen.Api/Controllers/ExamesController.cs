using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Cligen.Dominio.Comum;
using Cligen.Dominio.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Exames solicitados. Exclusão só lógica, com motivo; excluídos respondem 404.</summary>
[ApiController]
[Route("api/exames")]
public sealed class ExamesController(ExameService service) : ControllerBase
{
    // 50 MB do arquivo (P1) + folga para o envelope multipart. O padrão do Kestrel (30 MB) barraria o upload.
    private const long LimiteUploadBytes = FormatoArquivo.TamanhoMaximoBytes + 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<PaginaDto<ExameResumoDto>>> Buscar(
        [FromQuery] string? busca, [FromQuery] Guid? pacienteId, [FromQuery] EstadoExame? estado, [FromQuery] bool excluidos = false,
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        => Ok(await service.BuscarAsync(busca, pacienteId, estado, excluidos, pagina, tamanhoPagina, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExameDto>> Obter(Guid id, CancellationToken ct)
        => await service.ObterAsync(id, ct) is { } e ? Ok(e) : NotFound();

    [HttpPost]
    public async Task<ActionResult<ExameDto>> Criar([FromBody] SalvarExameRequest req, CancellationToken ct)
    {
        var criado = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExameDto>> Atualizar(Guid id, [FromBody] SalvarExameRequest req, CancellationToken ct)
        => Ok(await service.AtualizarAsync(id, req, ct));

    [HttpPost("{id:guid}/excluir")]
    public async Task<IActionResult> Excluir(Guid id, [FromBody] ExcluirExameRequest req, CancellationToken ct)
    {
        await service.ExcluirAsync(id, req, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/restaurar")]
    public async Task<ActionResult<ExameDto>> Restaurar(Guid id, CancellationToken ct)
        => Ok(await service.RestaurarAsync(id, ct));

    [HttpPost("{id:guid}/amostra/acolher")]
    public async Task<ActionResult<ExameDto>> AcolherAmostra(Guid id, [FromBody] AcolherAmostraRequest req, CancellationToken ct)
        => Ok(await service.AcolherAmostraAsync(id, req, ct));

    [HttpPost("{id:guid}/amostra/rejeitar")]
    public async Task<ActionResult<ExameDto>> RejeitarAmostra(Guid id, [FromBody] RejeitarAmostraRequest req, CancellationToken ct)
        => Ok(await service.RejeitarAmostraAsync(id, req, ct));

    [HttpPost("{id:guid}/anexos")]
    [RequestSizeLimit(LimiteUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = LimiteUploadBytes)]
    public async Task<ActionResult<AnexoDto>> EnviarAnexo(Guid id, IFormFile arquivo, CancellationToken ct)
    {
        // IFormFile acima de 64 KB é bufferizado em disco pelo ASP.NET Core: o stream permite Seek.
        await using var conteudo = arquivo.OpenReadStream();
        return Ok(await service.AdicionarAnexoAsync(id, conteudo, arquivo.FileName, arquivo.Length, ct));
    }

    [HttpGet("{id:guid}/anexos/{anexoId:guid}")]
    public async Task<IActionResult> BaixarAnexo(Guid id, Guid anexoId, CancellationToken ct)
        => await service.AbrirAnexoAsync(id, anexoId, ct) is { } r
            ? File(r.Conteudo, r.Anexo.TipoConteudo, r.Anexo.NomeOriginal)
            : NotFound();

    [HttpPost("{id:guid}/anexos/{anexoId:guid}/remover")]
    public async Task<IActionResult> RemoverAnexo(Guid id, Guid anexoId, CancellationToken ct)
    {
        await service.RemoverAnexoAsync(id, anexoId, ct);
        return NoContent();
    }
}

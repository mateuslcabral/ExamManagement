using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Parâmetros globais editáveis pela equipe.</summary>
[ApiController]
[Route("api/parametros")]
public sealed class ParametrosController(ParametroService service) : ControllerBase
{
    [HttpGet("dias-revisao")]
    public async Task<ActionResult<DiasRevisaoDto>> ObterDiasRevisao(CancellationToken ct)
        => Ok(await service.ObterDiasRevisaoAsync(ct));

    [HttpPut("dias-revisao")]
    public async Task<ActionResult<DiasRevisaoDto>> AlterarDiasRevisao([FromBody] DiasRevisaoDto req, CancellationToken ct)
        => Ok(await service.AlterarDiasRevisaoAsync(req, ct));
}

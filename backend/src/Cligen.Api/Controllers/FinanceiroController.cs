using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Cligen.Api.Controllers;

/// <summary>Financeiro: lançamentos por exame, desconto e relatório por regime de caixa. Sem exclusão física (P4).</summary>
[ApiController]
[Route("api")]
public sealed class FinanceiroController(PagamentoService service) : ControllerBase
{
    [HttpGet("exames/{exameId:guid}/financeiro")]
    public async Task<ActionResult<FinanceiroExameDto>> ResumoDoExame(Guid exameId, CancellationToken ct)
        => Ok(await service.ResumoDoExameAsync(exameId, ct));

    [HttpPost("exames/{exameId:guid}/pagamentos")]
    public async Task<ActionResult<FinanceiroExameDto>> Lancar(Guid exameId, [FromBody] LancarPagamentoRequest req, CancellationToken ct)
        => Ok(await service.LancarAsync(exameId, req, ct));

    [HttpPut("exames/{exameId:guid}/desconto")]
    public async Task<ActionResult<FinanceiroExameDto>> AplicarDesconto(Guid exameId, [FromBody] AplicarDescontoRequest req, CancellationToken ct)
        => Ok(await service.AplicarDescontoAsync(exameId, req, ct));

    [HttpPost("pagamentos/{id:guid}/receber")]
    public async Task<ActionResult<FinanceiroExameDto>> MarcarRecebido(Guid id, [FromBody] MarcarRecebidoRequest req, CancellationToken ct)
        => Ok(await service.MarcarRecebidoAsync(id, req, ct));

    [HttpPost("pagamentos/{id:guid}/glosar")]
    public async Task<ActionResult<FinanceiroExameDto>> Glosar(Guid id, [FromBody] MotivoRequest req, CancellationToken ct)
        => Ok(await service.GlosarAsync(id, req, ct));

    [HttpPost("pagamentos/{id:guid}/excluir")]
    public async Task<ActionResult<FinanceiroExameDto>> Excluir(Guid id, [FromBody] MotivoRequest req, CancellationToken ct)
        => Ok(await service.ExcluirAsync(id, req, ct));

    /// <summary>Regime de caixa (Q29). Sem período, usa o mês corrente (Brasília).</summary>
    [HttpGet("financeiro/relatorio")]
    public async Task<ActionResult<RelatorioFinanceiroDto>> Relatorio(
        [FromQuery] DateOnly? de, [FromQuery] DateOnly? ate, [FromQuery] Guid? convenioId, [FromQuery] Guid? origemId, CancellationToken ct)
    {
        var hoje = Cligen.Aplicacao.Comum.Relogio.Hoje();
        var inicio = de ?? new DateOnly(hoje.Year, hoje.Month, 1);
        var fim = ate ?? hoje;
        return Ok(await service.RelatorioAsync(inicio, fim, convenioId, origemId, ct));
    }
}

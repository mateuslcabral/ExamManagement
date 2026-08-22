using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Acolhimento e rejeição de amostra (CLG-ESP §7, P29–P31).</summary>
public sealed class AcolhimentoService(IExameRepositorio exames, ExameCatalogoService catalogo, IRelogio relogio)
{
    /// <summary>1→2. Data digitável com padrão hoje (P29); previsão gravada uma vez com os dias de revisão vigentes (P30).</summary>
    public async Task<ExameDto> AcolherAsync(Guid exameId, AcolherAmostraRequest req, CancellationToken ct = default)
    {
        var exame = await exames.ObterPorIdAsync(exameId, ct) ?? throw new RegistroNaoEncontradoException("Exame");
        var hoje = relogio.Hoje;
        var diasRevisao = await catalogo.ObterDiasRevisaoAsync(ct);

        exame.AcolherAmostra(req.DataAcolhimento ?? hoje, req.UsuarioId, diasRevisao, hoje);
        await exames.AtualizarAsync(exame, ct);
        return ExameService.MapearCompleto(exame);
    }

    /// <summary>2→1 (P31). A recoleta é um novo acolhimento.</summary>
    public async Task<ExameDto> RejeitarAsync(Guid exameId, RejeitarAmostraRequest req, CancellationToken ct = default)
    {
        var exame = await exames.ObterPorIdAsync(exameId, ct) ?? throw new RegistroNaoEncontradoException("Exame");
        exame.RejeitarAmostra(req.Motivo, req.UsuarioId);
        await exames.AtualizarAsync(exame, ct);
        return ExameService.MapearCompleto(exame);
    }
}

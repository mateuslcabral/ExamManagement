using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Servicos;

public sealed class ParametroService(IParametroRepositorio repositorio)
{
    public async Task<DiasRevisaoDto> ObterDiasRevisaoAsync(CancellationToken ct = default)
        => new((await ObterDiasRevisaoOuFalharAsync(ct)).ValorInteiro());

    /// <summary>
    /// Altera os dias de revisão de todos os exames (P13). Não recalcula previsões já gravadas em
    /// exames acolhidos: a data prevista é fixada no acolhimento (P14).
    /// </summary>
    public async Task<DiasRevisaoDto> AlterarDiasRevisaoAsync(DiasRevisaoDto req, CancellationToken ct = default)
    {
        var parametro = await ObterDiasRevisaoOuFalharAsync(ct);
        parametro.AlterarDiasRevisao(req.Dias);
        await repositorio.AtualizarAsync(parametro, ct);
        return new(parametro.ValorInteiro());
    }

    // Semeado pela migration; ausência é erro de implantação, não de uso.
    private async Task<Parametro> ObterDiasRevisaoOuFalharAsync(CancellationToken ct)
        => await repositorio.ObterAsync(Parametro.DiasRevisao, ct)
           ?? throw new InvalidOperationException($"Parâmetro '{Parametro.DiasRevisao}' não encontrado — migrations aplicadas?");
}

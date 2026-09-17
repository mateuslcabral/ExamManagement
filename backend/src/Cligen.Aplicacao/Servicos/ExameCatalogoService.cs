using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Catálogo de exames: CRUD mantido pela equipe (Q3). Exame nunca é apagado, só desativado.</summary>
public sealed class ExameCatalogoService(IExameCatalogoRepositorio repositorio, ParametroService parametros)
{
    public async Task<IReadOnlyList<ExameCatalogoDto>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default)
    {
        var diasRevisao = await DiasRevisaoAsync(ct);
        return (await repositorio.ListarAsync(apenasAtivos, ct)).Select(e => Mapear(e, diasRevisao)).ToList();
    }

    public async Task<ExameCatalogoDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await repositorio.ObterPorIdAsync(id, ct);
        return exame is null ? null : Mapear(exame, await DiasRevisaoAsync(ct));
    }

    public async Task<ExameCatalogoDto> CriarAsync(SalvarExameCatalogoRequest req, CancellationToken ct = default)
    {
        await GarantirNomeDisponivelAsync(req.Nome, null, ct);

        var exame = ExameCatalogo.Criar(req.Nome, req.PrazoExecucaoDias, req.PrecoReferencia);
        await repositorio.AdicionarAsync(exame, ct);
        return Mapear(exame, await DiasRevisaoAsync(ct));
    }

    /// <summary>
    /// Alterar prazo ou preço vale para os próximos exames solicitados: os já registrados guardam
    /// o preço próprio e a data prevista fixada no acolhimento (P14).
    /// </summary>
    public async Task<ExameCatalogoDto> AtualizarAsync(Guid id, SalvarExameCatalogoRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        await GarantirNomeDisponivelAsync(req.Nome, id, ct);

        exame.Atualizar(req.Nome, req.PrazoExecucaoDias, req.PrecoReferencia);
        await repositorio.AtualizarAsync(exame, ct);
        return Mapear(exame, await DiasRevisaoAsync(ct));
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Desativar();
        await repositorio.AtualizarAsync(exame, ct);
    }

    public async Task ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Reativar();
        await repositorio.AtualizarAsync(exame, ct);
    }

    private async Task GarantirNomeDisponivelAsync(string nome, Guid? ignorarId, CancellationToken ct)
    {
        nome = ExameCatalogo.NormalizarNome(nome);
        if (await repositorio.ExisteNomeAsync(nome, ignorarId, ct))
            throw new ExameCatalogoJaCadastradoException(nome);
    }

    private async Task<ExameCatalogo> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame não encontrado no catálogo.");

    private async Task<int> DiasRevisaoAsync(CancellationToken ct) => (await parametros.ObterDiasRevisaoAsync(ct)).Dias;

    private static ExameCatalogoDto Mapear(ExameCatalogo e, int diasRevisao) => new(
        e.Id, e.Nome, e.PrazoExecucaoDias, e.PrazoEntregaDias(diasRevisao), e.PrecoReferencia, e.Ativo, e.CriadoEm, e.AtualizadoEm);
}

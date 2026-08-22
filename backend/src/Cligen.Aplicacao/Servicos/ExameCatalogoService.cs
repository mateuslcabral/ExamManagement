using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

public sealed class ExameCatalogoService(IExameCatalogoRepositorio repositorio, IParametroRepositorio parametros)
{
    public async Task<IReadOnlyList<ExameCatalogoDto>> ListarAsync(bool somenteAtivos, CancellationToken ct = default)
    {
        var diasRevisao = await ObterDiasRevisaoAsync(ct);
        return (await repositorio.ListarAsync(somenteAtivos, ct)).Select(e => Mapear(e, diasRevisao)).ToList();
    }

    public async Task<ExameCatalogoDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var e = await repositorio.ObterPorIdAsync(id, ct);
        return e is null ? null : Mapear(e, await ObterDiasRevisaoAsync(ct));
    }

    public async Task<ExameCatalogoDto> CriarAsync(SalvarExameCatalogoRequest req, CancellationToken ct = default)
    {
        var item = ExameCatalogo.Criar(req.Nome, req.PrazoExecucaoDias, req.PrecoReferencia);
        if (await repositorio.ExisteNomeAsync(item.Nome, null, ct))
            throw new NomeDeExameJaCadastradoException(item.Nome);
        await repositorio.AdicionarAsync(item, ct);
        return Mapear(item, await ObterDiasRevisaoAsync(ct));
    }

    public async Task<ExameCatalogoDto> AtualizarAsync(Guid id, SalvarExameCatalogoRequest req, CancellationToken ct = default)
    {
        var item = await ObterOuFalharAsync(id, ct);
        item.Atualizar(req.Nome, req.PrazoExecucaoDias, req.PrecoReferencia);
        if (await repositorio.ExisteNomeAsync(item.Nome, id, ct))
            throw new NomeDeExameJaCadastradoException(item.Nome);
        await repositorio.AtualizarAsync(item, ct);
        return Mapear(item, await ObterDiasRevisaoAsync(ct));
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        var item = await ObterOuFalharAsync(id, ct);
        item.Desativar();
        await repositorio.AtualizarAsync(item, ct);
    }

    public async Task ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var item = await ObterOuFalharAsync(id, ct);
        item.Reativar();
        await repositorio.AtualizarAsync(item, ct);
    }

    // ---- Parâmetros ----

    public async Task<IReadOnlyList<ParametroDto>> ListarParametrosAsync(CancellationToken ct = default)
        => (await parametros.ListarAsync(ct)).Select(p => new ParametroDto(p.Chave, p.Valor, p.Descricao, p.AtualizadoEm)).ToList();

    public async Task<ParametroDto> AtualizarParametroAsync(string chave, AtualizarParametroRequest req, CancellationToken ct = default)
    {
        var p = await parametros.ObterAsync(chave, ct) ?? throw new RegistroNaoEncontradoException("Parâmetro");
        if (chave == Parametro.ChaveDiasRevisao && (!int.TryParse(req.Valor, out var dias) || dias < 0))
            throw new ValidacaoException("Os dias de revisão devem ser um número inteiro maior ou igual a zero.");
        p.AtualizarValor(req.Valor);
        await parametros.AtualizarAsync(p, ct);
        return new ParametroDto(p.Chave, p.Valor, p.Descricao, p.AtualizadoEm);
    }

    public async Task<int> ObterDiasRevisaoAsync(CancellationToken ct = default)
        => (await parametros.ObterAsync(Parametro.ChaveDiasRevisao, ct))?.ValorInteiro() ?? 3;

    private async Task<ExameCatalogo> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new RegistroNaoEncontradoException("Exame do catálogo");

    private static ExameCatalogoDto Mapear(ExameCatalogo e, int diasRevisao) => new(
        e.Id, e.Nome, e.PrazoExecucaoDias, diasRevisao, e.PrazoTotalDias(diasRevisao), e.PrecoReferencia, e.Ativo, e.CriadoEm, e.AtualizadoEm);
}

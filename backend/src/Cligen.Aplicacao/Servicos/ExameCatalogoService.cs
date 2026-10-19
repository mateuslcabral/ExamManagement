using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Catálogo de exames: CRUD mantido pela equipe (Q3). Exame nunca é apagado, só desativado.</summary>
public sealed class ExameCatalogoService(
    IExameCatalogoRepositorio repositorio,
    ICadastroRepositorio<Destino> destinos,
    IUsuarioAtual usuarioAtual)
{
    public async Task<IReadOnlyList<ExameCatalogoDto>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default)
    {
        var itens = await repositorio.ListarAsync(apenasAtivos, ct);
        var nomes = await NomesDosDestinosAsync(itens.Select(i => i.DestinoId), ct);
        return itens.Select(e => Mapear(e, nomes)).ToList();
    }

    public async Task<ExameCatalogoDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await repositorio.ObterPorIdAsync(id, ct);
        return exame is null ? null : Mapear(exame, await NomesDosDestinosAsync([exame.DestinoId], ct));
    }

    public async Task<ExameCatalogoDto> CriarAsync(SalvarExameCatalogoRequest req, CancellationToken ct = default)
    {
        await GarantirNomeDisponivelAsync(req.Nome, null, ct);
        await GarantirDestinoAsync(req.DestinoId, ct);

        var exame = ExameCatalogo.Criar(Dados(req), usuarioAtual.Id);
        await repositorio.AdicionarAsync(exame, ct);
        return Mapear(exame, await NomesDosDestinosAsync([exame.DestinoId], ct));
    }

    /// <summary>
    /// Alterar prazo ou preço vale para os próximos exames solicitados: os já registrados guardam
    /// o preço próprio e a data prevista fixada no acolhimento (P14).
    /// </summary>
    public async Task<ExameCatalogoDto> AtualizarAsync(Guid id, SalvarExameCatalogoRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        await GarantirNomeDisponivelAsync(req.Nome, id, ct);
        await GarantirDestinoAsync(req.DestinoId, ct);

        exame.Atualizar(Dados(req), usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
        return Mapear(exame, await NomesDosDestinosAsync([exame.DestinoId], ct));
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Desativar(usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
    }

    public async Task ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Reativar(usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
    }

    private static DadosCatalogo Dados(SalvarExameCatalogoRequest r)
        => new(r.Nome, r.PrazoDias, r.PrecoReferencia, r.PrecoLimite, r.NumeroGenes, r.Genes, r.DestinoId, r.InformacoesTecnicas);

    private async Task GarantirDestinoAsync(Guid? destinoId, CancellationToken ct)
    {
        if (destinoId is not { } id) return;
        var destino = await destinos.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Destino não encontrado.");
        if (!destino.Ativo) throw new ValidacaoException($"O destino '{destino.Nome}' está inativo.");
    }

    private async Task GarantirNomeDisponivelAsync(string nome, Guid? ignorarId, CancellationToken ct)
    {
        nome = ExameCatalogo.NormalizarNome(nome);
        if (await repositorio.ExisteNomeAsync(nome, ignorarId, ct))
            throw new ExameCatalogoJaCadastradoException(nome);
    }

    private async Task<ExameCatalogo> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame não encontrado no catálogo.");

    private async Task<Dictionary<Guid, string>> NomesDosDestinosAsync(IEnumerable<Guid?> ids, CancellationToken ct)
    {
        var distintos = ids.OfType<Guid>().Distinct().ToList();
        if (distintos.Count == 0) return [];
        // Lista pequena (dezenas de destinos): carregar tudo é mais simples que uma consulta por id.
        return (await destinos.ListarAsync(false, ct)).Where(d => distintos.Contains(d.Id)).ToDictionary(d => d.Id, d => d.Nome);
    }

    private static ExameCatalogoDto Mapear(ExameCatalogo e, IReadOnlyDictionary<Guid, string> destinos) => new(
        e.Id, e.Nome, e.PrazoDias, e.PrecoReferencia, e.PrecoLimite, e.NumeroGenes, e.Genes,
        e.DestinoId, e.DestinoId is { } d && destinos.TryGetValue(d, out var nome) ? nome : null,
        e.InformacoesTecnicas, e.Ativo, e.CriadoEm, e.AtualizadoEm);
}

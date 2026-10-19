using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>CRUD dos cadastros auxiliares. Item em uso nunca é apagado: é desativado e some das listas suspensas.</summary>
public sealed class CadastroService<T>(ICadastroRepositorio<T> repositorio, IUsuarioAtual usuarioAtual)
    where T : CadastroSimples, ICadastroCriavel<T>
{
    public async Task<IReadOnlyList<CadastroDto>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default)
        => (await repositorio.ListarAsync(apenasAtivos, ct)).Select(Mapear).ToList();

    public async Task<CadastroDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repositorio.ObterPorIdAsync(id, ct);
        return item is null ? null : Mapear(item);
    }

    public async Task<CadastroDto> CriarAsync(SalvarCadastroRequest req, CancellationToken ct = default)
    {
        await GarantirNomeDisponivelAsync(req.Nome, null, ct);
        var item = T.Criar(req.Nome, usuarioAtual.Id);
        await repositorio.AdicionarAsync(item, ct);
        return Mapear(item);
    }

    public async Task<CadastroDto> RenomearAsync(Guid id, SalvarCadastroRequest req, CancellationToken ct = default)
    {
        var item = await ObterOuFalharAsync(id, ct);
        await GarantirNomeDisponivelAsync(req.Nome, id, ct);
        item.Renomear(req.Nome, usuarioAtual.Id);
        await repositorio.AtualizarAsync(item, ct);
        return Mapear(item);
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        var item = await ObterOuFalharAsync(id, ct);
        item.Desativar(usuarioAtual.Id);
        await repositorio.AtualizarAsync(item, ct);
    }

    public async Task ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var item = await ObterOuFalharAsync(id, ct);
        item.Reativar(usuarioAtual.Id);
        await repositorio.AtualizarAsync(item, ct);
    }

    /// <summary>Para outros serviços: o item precisa existir e estar ativo para ser escolhido num cadastro novo.</summary>
    public async Task<T> ObterAtivoOuFalharAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException($"{Capitalizar(T.Rotulo)} não encontrado(a).");
        if (!item.Ativo) throw new ValidacaoException($"{Capitalizar(T.Rotulo)} '{item.Nome}' está inativo(a).");
        return item;
    }

    private async Task GarantirNomeDisponivelAsync(string nome, Guid? ignorarId, CancellationToken ct)
    {
        nome = CadastroSimples.NormalizarNome(nome);
        if (await repositorio.ExisteNomeAsync(nome, ignorarId, ct))
            throw new NomeJaCadastradoException(T.Rotulo, nome);
    }

    private async Task<T> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException($"{Capitalizar(T.Rotulo)} não encontrado(a).");

    private static string Capitalizar(string s) => char.ToUpperInvariant(s[0]) + s[1..];

    private static CadastroDto Mapear(T c) => new(c.Id, c.Nome, c.Ativo, c.CriadoEm, c.AtualizadoEm);
}

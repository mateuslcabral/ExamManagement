using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

public sealed class UsuarioService(
    IUsuarioRepositorio repositorio,
    IEnvioEmail envioEmail,
    ITokenDefinicaoSenha tokens,
    IHashSenha hashSenha,
    IUrlDefinicaoSenha urlDefinicao,
    IUsuarioAtual usuarioAtual)
{
    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken ct = default)
        => (await repositorio.ListarAsync(ct)).Select(Mapear).ToList();

    public async Task<UsuarioDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var u = await repositorio.ObterPorIdAsync(id, ct);
        return u is null ? null : Mapear(u);
    }

    /// <summary>
    /// Gestão de Usuários: criar. O tipo de login é decidido pela entidade a partir do e-mail.
    /// Conta Google nasce ativa. Conta local dispara e-mail para o próprio usuário definir a senha.
    /// </summary>
    public async Task<UsuarioDto> CriarAsync(CriarUsuarioRequest req, CancellationToken ct = default)
    {
        var email = Usuario.NormalizarEmail(req.Email);
        if (await repositorio.ExisteEmailAsync(email, ct))
            throw new EmailJaCadastradoException(email);

        var usuario = Usuario.Criar(req.Nome, email);
        await repositorio.AdicionarAsync(usuario, ct);

        if (usuario.TipoLogin == TipoLogin.Local)
            await EnviarLinkDefinicaoSenhaAsync(usuario, ct);

        return Mapear(usuario);
    }

    public async Task<UsuarioDto> AtualizarAsync(Guid id, AtualizarUsuarioRequest req, CancellationToken ct = default)
    {
        var usuario = await ObterOuFalharAsync(id, ct);
        usuario.AtualizarNome(req.Nome);
        await repositorio.AtualizarAsync(usuario, ct);
        return Mapear(usuario);
    }

    public async Task DesativarAsync(Guid id, CancellationToken ct = default)
    {
        // Evita que a equipe fique sem ninguém capaz de entrar por um clique acidental.
        if (id == usuarioAtual.Id)
            throw new ValidacaoException("Você não pode desativar a si mesmo.");

        var usuario = await ObterOuFalharAsync(id, ct);
        usuario.Desativar();
        await repositorio.AtualizarAsync(usuario, ct);
    }

    public async Task ReativarAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await ObterOuFalharAsync(id, ct);
        usuario.Reativar();
        await repositorio.AtualizarAsync(usuario, ct);
    }

    /// <summary>Reenvia o link de definição de senha (conta local que ainda não definiu, ou esqueceu).</summary>
    public async Task ReenviarDefinicaoSenhaAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await ObterOuFalharAsync(id, ct);
        if (usuario.TipoLogin != TipoLogin.Local)
            throw new OperacaoInvalidaParaTipoLoginException("reenviar definição de senha");
        await EnviarLinkDefinicaoSenhaAsync(usuario, ct);
    }

    public async Task DefinirSenhaAsync(DefinirSenhaRequest req, CancellationToken ct = default)
    {
        var usuarioId = tokens.Validar(req.Token) ?? throw new ValidacaoException("Link inválido ou expirado.");
        var usuario = await ObterOuFalharAsync(usuarioId, ct);

        ValidarForcaSenha(req.Senha);
        usuario.DefinirSenha(hashSenha.Gerar(req.Senha));
        await repositorio.AtualizarAsync(usuario, ct);
    }

    private async Task<Usuario> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Usuário não encontrado.");

    private async Task EnviarLinkDefinicaoSenhaAsync(Usuario usuario, CancellationToken ct)
    {
        var token = tokens.Gerar(usuario.Id);
        var link = urlDefinicao.Montar(token);
        await envioEmail.EnviarDefinicaoDeSenhaAsync(usuario.Email, usuario.Nome, link, ct);
    }

    // Política de senha ainda em aberto (docs/06-pendencias). Mínimo razoável até lá.
    private static void ValidarForcaSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
            throw new ValidacaoException("A senha deve ter pelo menos 8 caracteres.");
    }

    private static UsuarioDto Mapear(Usuario u) => new(
        u.Id, u.Nome, u.Email, u.TipoLogin, u.Ativo, u.SenhaHash is not null, u.CriadoEm, u.AtualizadoEm);
}

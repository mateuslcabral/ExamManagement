using Cligen.Dominio.Comum;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Membro da equipe Cligen com acesso ao sistema interno.
/// Nunca existe autocadastro: este registro só nasce a partir da Gestão de Usuários.
/// </summary>
public class Usuario
{
    public const string DominioGoogle = "gmail.com";

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public TipoLogin TipoLogin { get; private set; }

    /// <summary>Somente para <see cref="TipoLogin.Local"/>. Nulo até o usuário definir a senha.</summary>
    public string? SenhaHash { get; private set; }

    /// <summary>Somente para <see cref="TipoLogin.Google"/>. Vinculado no primeiro login.</summary>
    public string? GoogleSubjectId { get; private set; }

    /// <summary>Conta google: ativa na criação. Conta local: ativa após definir a senha.</summary>
    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    private Usuario() { } // EF Core

    public static Usuario Criar(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome é obrigatório.");

        email = NormalizarEmail(email);

        var tipo = email.EndsWith("@" + DominioGoogle, StringComparison.OrdinalIgnoreCase)
            ? TipoLogin.Google
            : TipoLogin.Local;

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Email = email,
            TipoLogin = tipo,
            // Regra: conta Google já nasce com acesso; conta local só depois de definir a senha.
            Ativo = tipo == TipoLogin.Google,
            CriadoEm = DateTime.UtcNow
        };
    }

    public void DefinirSenha(string senhaHash)
    {
        if (TipoLogin != TipoLogin.Local)
            throw new OperacaoInvalidaParaTipoLoginException("definir senha");
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ValidacaoException("Hash de senha inválido.");

        SenhaHash = senhaHash;
        Ativo = true;
        Tocar();
    }

    public void VincularGoogle(string subjectId)
    {
        if (TipoLogin != TipoLogin.Google)
            throw new OperacaoInvalidaParaTipoLoginException("vincular conta Google");
        if (string.IsNullOrWhiteSpace(subjectId))
            throw new ValidacaoException("Identificador Google inválido.");

        GoogleSubjectId = subjectId;
        Tocar();
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome é obrigatório.");
        Nome = nome.Trim();
        Tocar();
    }

    public void Desativar() { Ativo = false; Tocar(); }

    public void Reativar()
    {
        // Conta local sem senha definida não pode ser reativada "na marra": precisa redefinir a senha.
        if (TipoLogin == TipoLogin.Local && SenhaHash is null)
            throw new ValidacaoException("Este usuário ainda não definiu a senha; reenvie o link de definição.");
        Ativo = true;
        Tocar();
    }

    public bool PodeAutenticarComSenha() => Ativo && TipoLogin == TipoLogin.Local && SenhaHash is not null;

    public void GarantirAtivo()
    {
        if (!Ativo) throw new UsuarioInativoException();
    }

    private void Tocar() => AtualizadoEm = DateTime.UtcNow;

    public static string NormalizarEmail(string email) => Contato.NormalizarEmail(email);
}

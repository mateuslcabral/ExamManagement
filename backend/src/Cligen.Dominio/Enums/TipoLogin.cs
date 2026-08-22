namespace Cligen.Dominio.Enums;

/// <summary>
/// Como o usuário se autentica. Determinado automaticamente pelo domínio do e-mail
/// no momento da criação (ver docs/02-arquitetura/autenticacao.md).
/// </summary>
public enum TipoLogin
{
    /// <summary>Conta @gmail.com — entra via "Entrar com Google", sem senha.</summary>
    Google = 1,

    /// <summary>Qualquer outro e-mail — define a própria senha por link enviado na criação.</summary>
    Local = 2
}

namespace Cligen.Aplicacao.Interfaces.Servicos;

/// <summary>Hash e verificação de senha. Implementado na Infraestrutura com ASP.NET Core Identity.</summary>
public interface IHashSenha
{
    string Gerar(string senha);
    bool Verificar(string senha, string hash);
}

/// <summary>
/// Canal de e-mail transacional. Provedor concreto ainda em aberto (Q37) — a Infraestrutura
/// entrega por ora uma implementação que apenas registra em log.
/// </summary>
public interface IEnvioEmail
{
    Task EnviarDefinicaoDeSenhaAsync(string destinatario, string nome, string linkDefinicao, CancellationToken ct = default);
}

/// <summary>Token de uso único, com expiração, para o fluxo de definição/redefinição de senha.</summary>
public interface ITokenDefinicaoSenha
{
    string Gerar(Guid usuarioId);
    Guid? Validar(string token);
}

/// <summary>Monta a URL pública (no frontend) onde o usuário define a senha. Configurada na Api.</summary>
public interface IUrlDefinicaoSenha
{
    string Montar(string token);
}

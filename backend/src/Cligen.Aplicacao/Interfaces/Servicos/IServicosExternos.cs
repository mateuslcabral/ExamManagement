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

    /// <summary>Boas-vindas com endereço e instruções do portal (Q32). Texto final pendente (C7).</summary>
    Task EnviarBoasVindasPacienteAsync(BoasVindasPaciente mensagem, CancellationToken ct = default);
}

/// <summary>
/// WhatsApp Business API oficial, com templates aprovados pela Meta (Q36). Provedor ainda não integrado —
/// a Infraestrutura entrega por ora uma implementação que apenas registra em log.
/// </summary>
public interface IEnvioWhatsApp
{
    Task EnviarBoasVindasPacienteAsync(BoasVindasPaciente mensagem, CancellationToken ct = default);
}

/// <param name="NomeDestinatario">Responsável legal, quando houver (P9); senão, o próprio paciente.</param>
public sealed record BoasVindasPaciente(
    string Email, string Telefone, string NomeDestinatario, string NomePaciente, string UrlPortal);

/// <summary>
/// Armazenamento de objetos para anexos e laudos, fora do banco. Provedor definitivo em aberto — por ora, disco local.
/// Implementações gravam os bytes exatamente como recebidos (laudo assinado não pode ser reprocessado) e nunca apagam.
/// </summary>
public interface IArmazenamentoArquivos
{
    Task<ArquivoArmazenado> SalvarAsync(Stream conteudo, string extensao, CancellationToken ct = default);

    /// <exception cref="FileNotFoundException">Se não houver arquivo no caminho.</exception>
    Task<Stream> AbrirLeituraAsync(string caminho, CancellationToken ct = default);
}

/// <param name="Caminho">Chave opaca no armazenamento, guardada no banco.</param>
/// <param name="HashSha256">Hexadecimal minúsculo, calculado sobre os bytes gravados.</param>
public sealed record ArquivoArmazenado(string Caminho, long TamanhoBytes, string HashSha256);

/// <summary>Endereço público do portal do paciente, citado nas mensagens. Configurado na Api.</summary>
public interface IUrlPortalPaciente
{
    string Obter();
}

/// <summary>
/// Membro da equipe autenticado no BFF que originou a requisição. Usado para autoria (quem cadastrou,
/// quem alterou, quem excluiu). Indisponível nos endpoints de autenticação, onde ainda não há sessão.
/// </summary>
public interface IUsuarioAtual
{
    /// <exception cref="InvalidOperationException">Se a requisição não carrega usuário autenticado.</exception>
    Guid Id { get; }
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

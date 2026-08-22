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

    /// <summary>Boas-vindas ao paciente no cadastro (Q32). Texto provisório até C7 (P20).</summary>
    Task EnviarBoasVindasPacienteAsync(string destinatario, string nome, CancellationToken ct = default);

    /// <summary>Aviso de laudo disponível (D11/D13): modelo interno (convite de retorno) ou externo (simplificado). Texto provisório (P34).</summary>
    Task EnviarLaudoDisponivelAsync(string destinatario, string nome, string nomeExame, bool medicoInterno, CancellationToken ct = default);
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

/// <summary>
/// Canal WhatsApp (API oficial, templates aprovados pela Meta — Q36). Provedor/cliente ainda em aberto;
/// a Infraestrutura entrega por ora uma implementação que apenas registra em log (P20).
/// </summary>
public interface IEnvioWhatsApp
{
    Task EnviarBoasVindasAsync(string telefone, string nome, CancellationToken ct = default);
    Task EnviarLaudoDisponivelAsync(string telefone, string nome, string nomeExame, bool medicoInterno, CancellationToken ct = default);
}

/// <summary>Data de referência do domínio (hoje). Abstraída para testes e para fuso da clínica.</summary>
public interface IRelogio
{
    DateOnly Hoje { get; }
}

/// <summary>
/// Armazenamento de binários fora do banco. Implementação provisória em disco local (P27);
/// trocar por object storage quando a hospedagem for definida, sem tocar na Aplicação.
/// </summary>
public interface IArmazenamentoArquivos
{
    /// <summary>Grava o conteúdo e devolve o caminho/chave a persistir na entidade.</summary>
    Task<string> SalvarAsync(string caminhoRelativo, Stream conteudo, CancellationToken ct = default);
    Task<Stream> AbrirAsync(string caminho, CancellationToken ct = default);
    Task RemoverAsync(string caminho, CancellationToken ct = default);
}

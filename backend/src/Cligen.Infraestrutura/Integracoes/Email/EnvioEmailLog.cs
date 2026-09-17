using Cligen.Aplicacao.Interfaces.Servicos;
using Microsoft.Extensions.Logging;

namespace Cligen.Infraestrutura.Integracoes.Email;

/// <summary>
/// Implementação provisória: registra no log em vez de enviar. Provedor de e-mail transacional
/// e domínio de envio ainda em aberto (Q37). Trocar por SendGrid/SES/etc. sem tocar na Aplicação.
/// </summary>
public sealed class EnvioEmailLog(ILogger<EnvioEmailLog> logger) : IEnvioEmail
{
    public Task EnviarDefinicaoDeSenhaAsync(string destinatario, string nome, string linkDefinicao, CancellationToken ct = default)
    {
        logger.LogWarning(
            "[E-MAIL NÃO ENVIADO — provedor não configurado] Para: {Destinatario} ({Nome}). Link de definição de senha: {Link}",
            destinatario, nome, linkDefinicao);
        return Task.CompletedTask;
    }

    public Task EnviarBoasVindasPacienteAsync(BoasVindasPaciente mensagem, CancellationToken ct = default)
    {
        logger.LogWarning(
            "[E-MAIL NÃO ENVIADO — provedor não configurado] Para: {Destinatario} ({Nome}). Boas-vindas do paciente {Paciente}; portal: {Portal}",
            mensagem.Email, mensagem.NomeDestinatario, mensagem.NomePaciente, mensagem.UrlPortal);
        return Task.CompletedTask;
    }
}

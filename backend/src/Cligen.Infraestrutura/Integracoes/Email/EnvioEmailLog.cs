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

    public Task EnviarBoasVindasPacienteAsync(string destinatario, string nome, CancellationToken ct = default)
    {
        // Texto provisório — C7 ainda vai definir a mensagem de boas-vindas sem usuário/senha.
        logger.LogWarning(
            "[E-MAIL NÃO ENVIADO — provedor não configurado] Boas-vindas para: {Destinatario} ({Nome}).",
            destinatario, nome);
        return Task.CompletedTask;
    }

    public Task EnviarLaudoDisponivelAsync(string destinatario, string nome, string nomeExame, bool medicoInterno, CancellationToken ct = default)
    {
        // Dois modelos (D13). Texto provisório até C7/templates definitivos. Cita o nome do exame — risco R1 aceito (Q35).
        var modelo = medicoInterno
            ? "MODELO INTERNO — inclui convite para agendar retorno com o Dr. Arsonval Lamounier Junior e aconselhamento genético"
            : "MODELO EXTERNO — aviso simplificado";
        logger.LogWarning(
            "[E-MAIL NÃO ENVIADO — provedor não configurado] Laudo disponível para: {Destinatario} ({Nome}). Exame: {Exame}. {Modelo}. Portal: https://resultados.cligen.com.br",
            destinatario, nome, nomeExame, modelo);
        return Task.CompletedTask;
    }
}

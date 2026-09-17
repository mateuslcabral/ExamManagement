using Cligen.Aplicacao.Interfaces.Servicos;
using Microsoft.Extensions.Logging;

namespace Cligen.Infraestrutura.Integracoes.WhatsApp;

/// <summary>
/// Implementação provisória: registra no log em vez de enviar. A integração com a WhatsApp Business API depende
/// dos textos (C7) e da aprovação dos templates pela Meta. Trocar sem tocar na Aplicação.
/// </summary>
public sealed class EnvioWhatsAppLog(ILogger<EnvioWhatsAppLog> logger) : IEnvioWhatsApp
{
    public Task EnviarBoasVindasPacienteAsync(BoasVindasPaciente mensagem, CancellationToken ct = default)
    {
        logger.LogWarning(
            "[WHATSAPP NÃO ENVIADO — provedor não configurado] Para: {Telefone} ({Destinatario}). Boas-vindas do paciente {Paciente}; portal: {Portal}",
            mensagem.Telefone, mensagem.NomeDestinatario, mensagem.NomePaciente, mensagem.UrlPortal);
        return Task.CompletedTask;
    }

    public Task EnviarLaudoDisponivelAsync(LaudoDisponivel mensagem, CancellationToken ct = default)
    {
        logger.LogWarning(
            "[WHATSAPP NÃO ENVIADO — provedor não configurado] Para: {Telefone} ({Destinatario}). Laudo disponível: {Exame} de {Paciente}, modelo {Modelo}; portal: {Portal}",
            mensagem.Telefone, mensagem.NomeDestinatario, mensagem.NomeExame, mensagem.NomePaciente,
            mensagem.MedicoInterno ? "médico interno" : "médico externo", mensagem.UrlPortal);
        return Task.CompletedTask;
    }
}

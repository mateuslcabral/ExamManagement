using Cligen.Aplicacao.Interfaces.Servicos;
using Microsoft.Extensions.Logging;

namespace Cligen.Infraestrutura.Integracoes.WhatsApp;

/// <summary>
/// Implementação provisória: registra no log em vez de enviar. Cliente da WhatsApp Business API
/// e templates aprovados pela Meta ainda em aberto (Q36, C7). Trocar sem tocar na Aplicação.
/// </summary>
public sealed class EnvioWhatsAppLog(ILogger<EnvioWhatsAppLog> logger) : IEnvioWhatsApp
{
    public Task EnviarBoasVindasAsync(string telefone, string nome, CancellationToken ct = default)
    {
        logger.LogWarning(
            "[WHATSAPP NÃO ENVIADO — provedor não configurado] Boas-vindas para {Telefone} ({Nome}).",
            telefone, nome);
        return Task.CompletedTask;
    }

    public Task EnviarLaudoDisponivelAsync(string telefone, string nome, string nomeExame, bool medicoInterno, CancellationToken ct = default)
    {
        logger.LogWarning(
            "[WHATSAPP NÃO ENVIADO — provedor não configurado] Laudo disponível para {Telefone} ({Nome}). Exame: {Exame}. Template: {Template}",
            telefone, nome, nomeExame, medicoInterno ? "laudo_disponivel_interno" : "laudo_disponivel_externo");
        return Task.CompletedTask;
    }
}

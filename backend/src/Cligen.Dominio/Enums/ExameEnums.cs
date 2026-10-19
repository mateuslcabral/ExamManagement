namespace Cligen.Dominio.Enums;

/// <summary>Estados do fluxo do laudo (docs/05-regras-negocio/fluxo-laudo.md). A ordem numérica é a do fluxo.</summary>
public enum EstadoExame
{
    AguardandoAmostra = 1,
    AmostraAcolhida = 2,
    LaudoParceiroPronto = 3,
    LaudoCligenParaRevisao = 4,
    LaudoRevisado = 5,
    Disponibilizado = 6
}

/// <summary>As três etapas do laudo (D9). O valor numérico é o do estado que a etapa produz.</summary>
public enum TipoEtapaLaudo
{
    LaudoParceiroPronto = 3,
    LaudoCligenParaRevisao = 4,
    LaudoRevisado = 5
}

/// <summary>O que é o anexo (pedido do cliente em 17/10/2026; antes sem nomeação — Q22).</summary>
public enum TipoAnexo
{
    PedidoMedico = 1,
    TermoConsentimento = 2,
    Outro = 3
}

/// <summary>Define o modelo de mensagem na disponibilização do laudo (D11, D13).</summary>
public enum TipoMedico
{
    Interno = 1,
    Externo = 2
}

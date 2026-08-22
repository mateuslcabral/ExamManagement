namespace Cligen.Dominio.Enums;

/// <summary>Os seis estados do fluxo do laudo (CLG-ESP §8). Na v1 o exame nasce em 1 e só avança pelos módulos seguintes (P26).</summary>
public enum EstadoExame
{
    AguardandoAmostra = 1,
    AmostraAcolhida = 2,
    LaudoParceiroPronto = 3,
    LaudoCligenParaRevisao = 4,
    LaudoRevisado = 5,
    DisponibilizadoAoPaciente = 6
}

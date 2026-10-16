namespace Cligen.Dominio.Enums;

/// <summary>Métodos aceitos (docs/05-regras-negocio/financeiro.md). Lançamento manual, sem gateway (Q31).</summary>
public enum MetodoPagamento
{
    Pix = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    Dinheiro = 4,
    Boleto = 5,
    Transferencia = 6,
    Convenio = 7
}

/// <summary>
/// Só <see cref="Recebido"/> compõe o faturamento (regime de caixa — Q29). <see cref="Glosado"/> é a recusa do
/// convênio: sai da previsão sem apagar o lançamento (P11).
/// </summary>
public enum StatusPagamento
{
    AReceber = 1,
    Recebido = 2,
    Glosado = 3
}

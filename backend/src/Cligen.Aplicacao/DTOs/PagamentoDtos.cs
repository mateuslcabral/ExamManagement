using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record PagamentoDto(
    Guid Id,
    Guid ExameId,
    MetodoPagamento Metodo,
    decimal Valor,
    StatusPagamento Status,
    DateOnly? DataRecebimento,
    string? Convenio,
    string? NumeroGuia,
    int? Parcelas,
    decimal? ValorParcela,
    string? Observacao,
    string? MotivoGlosa,
    DateTime CriadoEm);

/// <param name="DataRecebimento">Só para particular; nula usa hoje. Convênio nasce a receber.</param>
public sealed record LancarPagamentoRequest(
    MetodoPagamento Metodo,
    decimal Valor,
    DateOnly? DataRecebimento,
    string? Convenio,
    string? NumeroGuia,
    int? Parcelas,
    decimal? ValorParcela,
    string? Observacao);

public sealed record MarcarRecebidoRequest(DateOnly DataRecebimento);

public sealed record MotivoRequest(string Motivo);

public sealed record AplicarDescontoRequest(decimal Desconto, string? Motivo);

/// <summary>Situação financeira de um exame: preço, desconto, o que já entrou e o que ainda falta.</summary>
public sealed record FinanceiroExameDto(
    Guid ExameId,
    decimal Preco,
    decimal Desconto,
    string? MotivoDesconto,
    decimal ValorAPagar,
    decimal Recebido,
    decimal AReceber,
    /// <summary>ValorAPagar − Recebido − AReceber. Positivo: falta lançar; negativo: lançado a mais.</summary>
    decimal Saldo,
    IReadOnlyList<PagamentoDto> Pagamentos);

/// <summary>Linha de relatório, com paciente e exame já resolvidos na consulta.</summary>
public sealed record LinhaFinanceiraDto(
    Guid PagamentoId,
    Guid ExameId,
    string ExameNome,
    string PacienteNome,
    TipoDocumento PacienteTipoDocumento,
    string PacienteNumeroDocumento,
    MetodoPagamento Metodo,
    string? Convenio,
    string? NumeroGuia,
    decimal Valor,
    DateOnly? DataRecebimento,
    DateOnly DataEntradaExame);

public sealed record TotalPorChaveDto(string Chave, decimal Total, int Quantidade);

/// <summary>Regime de caixa (Q29): recebidos pela data de recebimento no período; a receber de convênio, sem corte de data.</summary>
public sealed record RelatorioFinanceiroDto(
    DateOnly De,
    DateOnly Ate,
    decimal TotalRecebido,
    IReadOnlyList<TotalPorChaveDto> RecebidoPorMetodo,
    IReadOnlyList<LinhaFinanceiraDto> Recebidos,
    decimal TotalAReceber,
    IReadOnlyList<TotalPorChaveDto> AReceberPorConvenio,
    IReadOnlyList<LinhaFinanceiraDto> AReceber);

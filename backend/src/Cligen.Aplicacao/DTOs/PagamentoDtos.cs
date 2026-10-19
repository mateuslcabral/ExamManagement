using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record PagamentoDto(
    Guid Id,
    Guid ExameId,
    MetodoPagamento Metodo,
    decimal Valor,
    StatusPagamento Status,
    DateOnly? DataRecebimento,
    Guid? ConvenioId,
    string? ConvenioNome,
    string? NumeroGuia,
    int? Parcelas,
    decimal? ValorParcela,
    string? Observacao,
    string? MotivoGlosa,
    DateTime CriadoEm);

/// <param name="DataRecebimento">Só para particular; nula usa hoje. Convênio nasce a receber.</param>
/// <param name="ConvenioId">Obrigatório quando o método é convênio (cadastro).</param>
public sealed record LancarPagamentoRequest(
    MetodoPagamento Metodo,
    decimal Valor,
    DateOnly? DataRecebimento,
    Guid? ConvenioId,
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
    decimal PrecoLimite,
    decimal ValorAPagar,
    decimal Recebido,
    decimal AReceber,
    /// <summary>ValorAPagar − Recebido − AReceber. Positivo: falta lançar; negativo: lançado a mais.</summary>
    decimal Saldo,
    IReadOnlyList<PagamentoDto> Pagamentos);

/// <summary>Linha de relatório, com paciente, exame, origem e convênio já resolvidos na consulta.</summary>
public sealed record LinhaFinanceiraDto(
    Guid PagamentoId,
    Guid ExameId,
    string ExameCodigo,
    string ExameNome,
    string PacienteNome,
    TipoDocumento PacienteTipoDocumento,
    string PacienteNumeroDocumento,
    Guid OrigemId,
    string OrigemNome,
    MetodoPagamento Metodo,
    Guid? ConvenioId,
    string? ConvenioNome,
    string? NumeroGuia,
    decimal Valor,
    DateOnly? DataRecebimento,
    DateOnly DataEntradaExame);

public sealed record TotalPorChaveDto(string Chave, decimal Total, int Quantidade);

/// <summary>Regime de caixa (Q29): recebidos pela data de recebimento no período; a receber de convênio, sem corte de data.</summary>
public sealed record RelatorioFinanceiroDto(
    DateOnly De,
    DateOnly Ate,
    Guid? ConvenioId,
    Guid? OrigemId,
    decimal TotalRecebido,
    IReadOnlyList<TotalPorChaveDto> RecebidoPorMetodo,
    IReadOnlyList<TotalPorChaveDto> RecebidoPorOrigem,
    IReadOnlyList<LinhaFinanceiraDto> Recebidos,
    decimal TotalAReceber,
    IReadOnlyList<TotalPorChaveDto> AReceberPorConvenio,
    IReadOnlyList<LinhaFinanceiraDto> AReceber);

namespace Cligen.Aplicacao.DTOs;

/// <param name="PrazoDias">Prazo de entrega em dias corridos a partir do acolhimento; já inclui a revisão.</param>
/// <param name="PrecoLimite">Piso: o valor a pagar de um exame não fica abaixo dele.</param>
public sealed record ExameCatalogoDto(
    Guid Id,
    string Nome,
    int PrazoDias,
    decimal PrecoReferencia,
    decimal PrecoLimite,
    int? NumeroGenes,
    string? Genes,
    Guid? DestinoId,
    string? DestinoNome,
    string? InformacoesTecnicas,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

/// <summary>Usado tanto na criação quanto na edição: os campos editáveis são os mesmos.</summary>
public sealed record SalvarExameCatalogoRequest(
    string Nome,
    int PrazoDias,
    decimal PrecoReferencia,
    decimal PrecoLimite,
    int? NumeroGenes,
    string? Genes,
    Guid? DestinoId,
    string? InformacoesTecnicas);

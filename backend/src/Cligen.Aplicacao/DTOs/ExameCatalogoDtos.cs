namespace Cligen.Aplicacao.DTOs;

/// <param name="PrazoEntregaDias">Prazo de execução + dias de revisão vigentes — o que o paciente de fato espera.</param>
public sealed record ExameCatalogoDto(
    Guid Id,
    string Nome,
    int PrazoExecucaoDias,
    int PrazoEntregaDias,
    decimal PrecoReferencia,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

/// <summary>Usado tanto na criação quanto na edição: os campos editáveis são os mesmos.</summary>
public sealed record SalvarExameCatalogoRequest(string Nome, int PrazoExecucaoDias, decimal PrecoReferencia);

public sealed record DiasRevisaoDto(int Dias);

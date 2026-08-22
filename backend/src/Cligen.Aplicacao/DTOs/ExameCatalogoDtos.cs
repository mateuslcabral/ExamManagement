namespace Cligen.Aplicacao.DTOs;

public sealed record ExameCatalogoDto(
    Guid Id,
    string Nome,
    int PrazoExecucaoDias,
    int DiasRevisao,
    int PrazoTotalDias,
    decimal PrecoReferencia,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record SalvarExameCatalogoRequest(string Nome, int PrazoExecucaoDias, decimal PrecoReferencia);

public sealed record ParametroDto(string Chave, string Valor, string Descricao, DateTime? AtualizadoEm);

public sealed record AtualizarParametroRequest(string Valor);

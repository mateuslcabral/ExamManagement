using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record AmostraDto(
    Guid Id,
    DateOnly DataAcolhimento,
    int PrazoExecucaoDias,
    int DiasRevisao,
    DateOnly DataLiberacaoPrevista,
    bool Recoleta,
    DateTime RegistradoEm,
    DateTime? RejeitadaEm,
    string? MotivoRejeicao);

public sealed record AcolherAmostraRequest(DateOnly DataAcolhimento);

public sealed record RejeitarAmostraRequest(string Motivo);

public sealed record AnexoDto(Guid Id, string NomeOriginal, string TipoConteudo, long TamanhoBytes, string HashSha256, DateTime EnviadoEm);

/// <summary>Linha da listagem: já traz nomes de paciente e exame, resolvidos na consulta.</summary>
public sealed record ExameResumoDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    TipoDocumento PacienteTipoDocumento,
    string PacienteNumeroDocumento,
    Guid ExameCatalogoId,
    string ExameNome,
    OrigemExame Origem,
    DateOnly DataEntrada,
    EstadoExame Estado,
    DateOnly? DataLiberacaoPrevista);

public sealed record ExameDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    TipoDocumento PacienteTipoDocumento,
    string PacienteNumeroDocumento,
    Guid ExameCatalogoId,
    string ExameNome,
    int PrazoExecucaoDias,
    int PrazoEntregaDias,
    OrigemExame Origem,
    string? Destino,
    TipoMedico TipoMedico,
    string NomeMedico,
    decimal Preco,
    DateOnly DataEntrada,
    EstadoExame Estado,
    DateOnly? DataLiberacaoPrevista,
    IReadOnlyList<AmostraDto> Amostras,
    IReadOnlyList<AnexoDto> Anexos,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

/// <param name="Preco">Nulo na criação usa o preço de referência do catálogo. Obrigatório na edição.</param>
public sealed record SalvarExameRequest(
    Guid PacienteId,
    Guid ExameCatalogoId,
    OrigemExame Origem,
    string? Destino,
    TipoMedico TipoMedico,
    string? NomeMedicoExterno,
    decimal? Preco);

public sealed record ExcluirExameRequest(string Motivo);

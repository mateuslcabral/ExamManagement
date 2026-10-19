using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record AmostraDto(
    Guid Id,
    DateOnly DataAcolhimento,
    int PrazoDias,
    DateOnly DataLiberacaoPrevista,
    bool Recoleta,
    DateTime RegistradoEm,
    DateTime? RejeitadaEm,
    string? MotivoRejeicao);

public sealed record AcolherAmostraRequest(DateOnly DataAcolhimento);

public sealed record RejeitarAmostraRequest(string Motivo);

public sealed record EtapaLaudoDto(
    Guid Id,
    TipoEtapaLaudo Tipo,
    DateTime Data,
    string NomeOriginal,
    long TamanhoBytes,
    string HashSha256,
    int Substituicoes,
    DateTime? SubstituidoEm);

public sealed record AnexoDto(
    Guid Id, TipoAnexo Tipo, string NomeOriginal, string TipoConteudo, long TamanhoBytes, string HashSha256, DateTime EnviadoEm);

public sealed record ComentarioDto(Guid Id, string Texto, Guid AutorId, string AutorNome, DateTime CriadoEm);

public sealed record AdicionarComentarioRequest(string Texto);

/// <summary>Campo de data usado nos filtros das filas (Fluxos).</summary>
public enum CampoDataExame
{
    Acolhimento = 1,
    Prevista = 2,
    Disponibilizado = 3
}

/// <summary>Linha da listagem: já traz nomes de paciente, exame, origem e destino, resolvidos na consulta.</summary>
public sealed record ExameResumoDto(
    Guid Id,
    string Codigo,
    Guid PacienteId,
    string PacienteNome,
    TipoDocumento PacienteTipoDocumento,
    string PacienteNumeroDocumento,
    Guid ExameCatalogoId,
    string ExameNome,
    Guid OrigemId,
    string OrigemNome,
    Guid? DestinoId,
    string? DestinoNome,
    DateOnly DataEntrada,
    EstadoExame Estado,
    DateOnly? DataAcolhimento,
    DateOnly? DataLiberacaoPrevista,
    DateTime? DataLiberacaoEfetiva,
    /// <summary>Preenchidos só na listagem de excluídos.</summary>
    DateTime? ExcluidoEm,
    string? MotivoExclusao);

public sealed record ExameDto(
    Guid Id,
    string Codigo,
    Guid PacienteId,
    string PacienteNome,
    TipoDocumento PacienteTipoDocumento,
    string PacienteNumeroDocumento,
    Guid ExameCatalogoId,
    string ExameNome,
    int PrazoDias,
    decimal PrecoLimite,
    Guid OrigemId,
    string OrigemNome,
    Guid? DestinoId,
    string? DestinoNome,
    TipoMedico TipoMedico,
    string NomeMedico,
    string? CrmMedico,
    string? UfCrm,
    decimal Preco,
    decimal Desconto,
    string? MotivoDesconto,
    decimal ValorAPagar,
    DateOnly DataEntrada,
    EstadoExame Estado,
    DateOnly? DataAcolhimento,
    DateOnly? DataLiberacaoPrevista,
    DateTime? DataLiberacaoEfetiva,
    IReadOnlyList<AmostraDto> Amostras,
    IReadOnlyList<EtapaLaudoDto> Etapas,
    IReadOnlyList<AnexoDto> Anexos,
    IReadOnlyList<ComentarioDto> Comentarios,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

/// <param name="DestinoId">Nulo na criação usa o destino padrão do catálogo.</param>
/// <param name="Preco">Nulo na criação usa o preço de referência do catálogo. Obrigatório na edição.</param>
public sealed record SalvarExameRequest(
    Guid PacienteId,
    Guid ExameCatalogoId,
    Guid OrigemId,
    Guid? DestinoId,
    TipoMedico TipoMedico,
    string? NomeMedicoExterno,
    string? CrmMedico,
    string? UfCrm,
    decimal? Preco);

public sealed record ExcluirExameRequest(string Motivo);

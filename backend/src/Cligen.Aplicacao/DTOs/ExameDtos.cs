using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record AnexoDto(Guid Id, string NomeOriginal, string TipoConteudo, long TamanhoBytes, DateTime EnviadoEm);

public sealed record ExameDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    Guid ExameCatalogoId,
    string ExameNome,
    int PrazoExecucaoDias,
    OrigemExame Origem,
    string? Destino,
    TipoMedicoSolicitante TipoMedico,
    string NomeMedico,
    DateOnly DataEntrada,
    decimal Preco,
    EstadoExame Estado,
    DateOnly? DataLiberacaoPrevista,
    DateTime? DataLiberacaoEfetiva,
    IReadOnlyList<AnexoDto> Anexos,
    AmostraDto? AmostraAtiva,
    IReadOnlyList<AmostraDto> Amostras,
    IReadOnlyList<EtapaLaudoDto> Etapas,
    bool Excluido,
    DateTime? ExcluidoEm,
    string? MotivoExclusao,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

/// <summary>Linha da listagem.</summary>
public sealed record ExameResumoDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    string ExameNome,
    OrigemExame Origem,
    TipoMedicoSolicitante TipoMedico,
    string NomeMedico,
    DateOnly DataEntrada,
    decimal Preco,
    EstadoExame Estado,
    DateOnly? DataLiberacaoPrevista,
    int QuantidadeAnexos,
    bool Excluido);

public sealed record CriarExameRequest(
    Guid PacienteId,
    Guid ExameCatalogoId,
    OrigemExame Origem,
    string? Destino,
    TipoMedicoSolicitante TipoMedico,
    string? NomeMedico,
    decimal? Preco);

public sealed record AtualizarExameRequest(
    OrigemExame Origem,
    string? Destino,
    TipoMedicoSolicitante TipoMedico,
    string? NomeMedico,
    decimal Preco);

public sealed record ExcluirExameRequest(Guid UsuarioId, string Motivo);

/// <summary>Arquivo aberto para download: quem consome é responsável por fechar o stream.</summary>
public sealed record ArquivoAnexo(Stream Conteudo, string NomeOriginal, string TipoConteudo);

// ---- Amostra e laudo ----

public sealed record AmostraDto(
    Guid Id,
    DateOnly DataAcolhimento,
    SituacaoAmostra Situacao,
    string? MotivoRejeicao,
    DateTime? RejeitadaEm,
    DateTime CriadoEm);

public sealed record EtapaLaudoDto(
    TipoEtapaLaudo Tipo,
    DateTime Data,
    string NomeOriginal,
    long TamanhoBytes,
    DateTime? ArquivoSubstituidoEm,
    int Substituicoes);

public sealed record AcolherAmostraRequest(Guid UsuarioId, DateOnly? DataAcolhimento);

public sealed record RejeitarAmostraRequest(Guid UsuarioId, string Motivo);

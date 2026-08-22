using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record ResponsavelLegalDto(
    string Nome,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string NumeroDocumentoFormatado,
    string? Parentesco);

public sealed record PacienteDto(
    Guid Id,
    string Nome,
    DateOnly DataNascimento,
    int Idade,
    bool MenorDeIdade,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string NumeroDocumentoFormatado,
    string Email,
    string Telefone,
    ResponsavelLegalDto? ResponsavelLegal,
    bool Excluido,
    DateTime? ExcluidoEm,
    string? MotivoExclusao,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

/// <summary>Linha da listagem — sem responsável nem auditoria.</summary>
public sealed record PacienteResumoDto(
    Guid Id,
    string Nome,
    DateOnly DataNascimento,
    int Idade,
    bool MenorDeIdade,
    TipoDocumento TipoDocumento,
    string NumeroDocumentoFormatado,
    string Email,
    string Telefone,
    bool Excluido,
    DateTime CriadoEm);

public sealed record ResponsavelLegalRequest(
    string Nome,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string? Parentesco);

public sealed record SalvarPacienteRequest(
    string Nome,
    DateOnly DataNascimento,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Email,
    string Telefone,
    ResponsavelLegalRequest? ResponsavelLegal);

/// <summary>Exclusão lógica (P16): autor é o usuário logado no BFF, motivo obrigatório (P10).</summary>
public sealed record ExcluirPacienteRequest(Guid UsuarioId, string Motivo);

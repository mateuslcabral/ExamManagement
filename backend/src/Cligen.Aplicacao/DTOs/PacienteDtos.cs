using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record ResponsavelLegalDto(string Nome, TipoDocumento TipoDocumento, string NumeroDocumento, string? Parentesco);

public sealed record PacienteDto(
    Guid Id,
    string Nome,
    DateOnly DataNascimento,
    bool MenorDeIdade,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Email,
    string Telefone,
    ResponsavelLegalDto? ResponsavelLegal,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

/// <summary>Usado na criação e na edição. E-mail e telefone de menor são os do responsável (P9).</summary>
public sealed record SalvarPacienteRequest(
    string Nome,
    DateOnly DataNascimento,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    string Email,
    string Telefone,
    ResponsavelLegalDto? ResponsavelLegal);

public sealed record PaginaDto<T>(IReadOnlyList<T> Itens, int Total, int Pagina, int TamanhoPagina);

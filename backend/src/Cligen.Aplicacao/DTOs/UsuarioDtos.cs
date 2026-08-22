using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.DTOs;

public sealed record UsuarioDto(
    Guid Id,
    string Nome,
    string Email,
    TipoLogin TipoLogin,
    bool Ativo,
    bool SenhaDefinida,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CriarUsuarioRequest(string Nome, string Email);

public sealed record AtualizarUsuarioRequest(string Nome);

public sealed record DefinirSenhaRequest(string Token, string Senha);

public sealed record ValidarCredenciaisRequest(string Email, string Senha);

/// <summary>Identidade mínima devolvida ao BFF após login bem-sucedido.</summary>
public sealed record IdentidadeDto(Guid Id, string Nome, string Email, TipoLogin TipoLogin);

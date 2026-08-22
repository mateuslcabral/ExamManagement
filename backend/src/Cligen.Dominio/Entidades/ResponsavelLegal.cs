using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;
using Cligen.Dominio.ValueObjects;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Responsável legal de paciente menor de idade (Q7). Faz parte do agregado <see cref="Paciente"/>;
/// o contato (e-mail/telefone) do cadastro já é o do responsável (P9/P19), por isso não se repete aqui.
/// </summary>
public class ResponsavelLegal
{
    public Guid Id { get; private set; }
    public Guid PacienteId { get; private set; }
    public string Nome { get; private set; } = null!;
    public TipoDocumento TipoDocumento { get; private set; }
    public string NumeroDocumento { get; private set; } = null!;
    public string? Parentesco { get; private set; }

    private ResponsavelLegal() { } // EF Core

    internal static ResponsavelLegal Criar(Guid pacienteId, string nome, TipoDocumento tipoDocumento, string numeroDocumento, string? parentesco)
    {
        var r = new ResponsavelLegal { Id = Guid.NewGuid(), PacienteId = pacienteId };
        r.Atualizar(nome, tipoDocumento, numeroDocumento, parentesco);
        return r;
    }

    internal void Atualizar(string nome, TipoDocumento tipoDocumento, string numeroDocumento, string? parentesco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome do responsável legal é obrigatório.");

        Nome = nome.Trim();
        TipoDocumento = tipoDocumento;
        NumeroDocumento = Documento.Normalizar(tipoDocumento, numeroDocumento);
        Parentesco = string.IsNullOrWhiteSpace(parentesco) ? null : parentesco.Trim();
    }
}

using Cligen.Dominio.Comum;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Responsável legal do paciente, acessado sempre através de <see cref="Paciente"/>. Obrigatório para menor (Q7).
/// O e-mail e o telefone do cadastro do paciente são os do responsável (P9), por isso não se repetem aqui.
/// </summary>
public class ResponsavelLegal
{
    public const int TamanhoMaximoNome = 200;
    public const int TamanhoMaximoParentesco = 50;

    public string Nome { get; private set; } = null!;
    public TipoDocumento TipoDocumento { get; private set; }
    public string NumeroDocumento { get; private set; } = null!;

    /// <summary>Texto livre e opcional (mãe, pai, tutor, curador...).</summary>
    public string? Parentesco { get; private set; }

    private ResponsavelLegal() { } // EF Core

    public Documento Documento => Documento.Criar(TipoDocumento, NumeroDocumento);

    public static ResponsavelLegal Criar(string? nome, Documento documento, string? parentesco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome do responsável legal é obrigatório.");
        nome = nome.Trim();
        if (nome.Length > TamanhoMaximoNome)
            throw new ValidacaoException($"O nome do responsável deve ter no máximo {TamanhoMaximoNome} caracteres.");

        parentesco = string.IsNullOrWhiteSpace(parentesco) ? null : parentesco.Trim();
        if (parentesco?.Length > TamanhoMaximoParentesco)
            throw new ValidacaoException($"O parentesco deve ter no máximo {TamanhoMaximoParentesco} caracteres.");

        return new ResponsavelLegal
        {
            Nome = nome,
            TipoDocumento = documento.Tipo,
            NumeroDocumento = documento.Numero,
            Parentesco = parentesco
        };
    }
}

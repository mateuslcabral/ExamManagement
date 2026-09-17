using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Comum;

/// <summary>CPF ou passaporte, sempre normalizado: CPF só com dígitos; passaporte em maiúsculas, sem separadores.</summary>
public sealed record Documento
{
    public TipoDocumento Tipo { get; }
    public string Numero { get; }

    private Documento(TipoDocumento tipo, string numero) => (Tipo, Numero) = (tipo, numero);

    public static Documento Criar(TipoDocumento tipo, string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ValidacaoException("O número do documento é obrigatório.");

        return tipo switch
        {
            TipoDocumento.Cpf => new(tipo, NormalizarCpf(numero)),
            TipoDocumento.Passaporte => new(tipo, NormalizarPassaporte(numero)),
            _ => throw new ValidacaoException("Tipo de documento inválido.")
        };
    }

    public string Descricao => Tipo == TipoDocumento.Cpf ? "CPF" : "passaporte";

    private static string NormalizarCpf(string numero)
    {
        var digitos = new string(numero.Where(char.IsAsciiDigit).ToArray());
        if (digitos.Length != 11 || digitos.Distinct().Count() == 1 || !DigitosVerificadoresConferem(digitos))
            throw new ValidacaoException("CPF inválido.");
        return digitos;
    }

    private static bool DigitosVerificadoresConferem(string cpf)
    {
        int Digito(int tamanho)
        {
            var soma = 0;
            for (var i = 0; i < tamanho; i++) soma += (cpf[i] - '0') * (tamanho + 1 - i);
            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
        return Digito(9) == cpf[9] - '0' && Digito(10) == cpf[10] - '0';
    }

    private static string NormalizarPassaporte(string numero)
    {
        var limpo = new string(numero.Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '.').ToArray()).ToUpperInvariant();
        if (limpo.Length is < 5 or > 20 || !limpo.All(char.IsAsciiLetterOrDigit))
            throw new ValidacaoException("Passaporte inválido: use de 5 a 20 letras e números.");
        return limpo;
    }
}

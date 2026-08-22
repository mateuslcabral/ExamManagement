using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.ValueObjects;

/// <summary>
/// Normalização e validação de documento (P18): CPF só dígitos, validado pelos dígitos verificadores;
/// passaporte alfanumérico de 6 a 20 caracteres, em maiúsculas.
/// </summary>
public static class Documento
{
    public static string Normalizar(TipoDocumento tipo, string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ValidacaoException("O número do documento é obrigatório.");

        return tipo switch
        {
            TipoDocumento.Cpf => NormalizarCpf(numero),
            TipoDocumento.Passaporte => NormalizarPassaporte(numero),
            _ => throw new ValidacaoException("Tipo de documento inválido.")
        };
    }

    private static string NormalizarCpf(string numero)
    {
        var digitos = new string(numero.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11 || digitos.Distinct().Count() == 1 || !DigitosVerificadoresValidos(digitos))
            throw new ValidacaoException("O CPF informado não é válido.");
        return digitos;
    }

    private static bool DigitosVerificadoresValidos(string cpf)
    {
        static int Calcular(string parte, int pesoInicial)
        {
            var soma = 0;
            for (var i = 0; i < parte.Length; i++)
                soma += (parte[i] - '0') * (pesoInicial - i);
            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        var d1 = Calcular(cpf[..9], 10);
        var d2 = Calcular(cpf[..10], 11);
        return cpf[9] - '0' == d1 && cpf[10] - '0' == d2;
    }

    private static string NormalizarPassaporte(string numero)
    {
        var limpo = new string(numero.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (limpo.Length is < 6 or > 20)
            throw new ValidacaoException("O número do passaporte deve ter entre 6 e 20 caracteres alfanuméricos.");
        return limpo;
    }

    /// <summary>Formatação para exibição (CPF com pontuação; passaporte como está).</summary>
    public static string Formatar(TipoDocumento tipo, string numero)
        => tipo == TipoDocumento.Cpf && numero.Length == 11
            ? $"{numero[..3]}.{numero[3..6]}.{numero[6..9]}-{numero[9..]}"
            : numero;
}

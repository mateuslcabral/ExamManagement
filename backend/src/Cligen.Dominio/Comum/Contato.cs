using System.Net.Mail;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Comum;

/// <summary>Normalização de e-mail e telefone, destinos dos disparos (Q10).</summary>
public static class Contato
{
    public static string NormalizarEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidacaoException("O e-mail é obrigatório.");
        email = email.Trim().ToLowerInvariant();
        bool valido;
        try { valido = new MailAddress(email).Address == email; }
        catch { valido = false; }
        // MailAddress aceita "a@b" (RFC); para uso real exigimos domínio com ponto.
        if (!valido || !email[(email.LastIndexOf('@') + 1)..].Contains('.'))
            throw new ValidacaoException($"O e-mail '{email}' não é válido.");
        return email;
    }

    /// <summary>
    /// Telefone em formato internacional (+DDI…), como a API do WhatsApp exige.
    /// Sem "+", 10 ou 11 dígitos são tratados como número brasileiro com DDD.
    /// </summary>
    public static string NormalizarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ValidacaoException("O telefone é obrigatório.");

        var internacional = telefone.TrimStart().StartsWith('+');
        var digitos = new string(telefone.Where(char.IsAsciiDigit).ToArray());

        if (!internacional && digitos.Length is 10 or 11)
            digitos = "55" + digitos;

        if (digitos.Length is < 10 or > 15 || (!internacional && !digitos.StartsWith("55"))
            || (digitos.StartsWith("55") && !NumeroBrasileiroValido(digitos[2..])))
            throw new ValidacaoException("Telefone inválido: informe DDD e número, ou o número internacional com +DDI.");

        return "+" + digitos;
    }

    /// <summary>DDD sem zero + celular (9 + 8 dígitos) ou fixo (8 dígitos começando de 2 a 5).</summary>
    private static bool NumeroBrasileiroValido(string nacional)
        => nacional.Length switch
        {
            11 => nacional[0] != '0' && nacional[1] != '0' && nacional[2] == '9',
            10 => nacional[0] != '0' && nacional[1] != '0' && nacional[2] is >= '2' and <= '5',
            _ => false
        };
}

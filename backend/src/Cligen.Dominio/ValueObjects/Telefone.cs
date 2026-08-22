using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.ValueObjects;

/// <summary>Telefone só com dígitos (P21): DDI opcional + DDD + número, 10 a 13 dígitos.</summary>
public static class Telefone
{
    public static string Normalizar(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ValidacaoException("O telefone é obrigatório.");

        var digitos = new string(telefone.Where(char.IsDigit).ToArray());
        if (digitos.Length is < 10 or > 13)
            throw new ValidacaoException("O telefone deve ter DDD e número (10 a 13 dígitos).");
        return digitos;
    }
}

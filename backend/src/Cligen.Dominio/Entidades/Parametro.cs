using System.Globalization;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Configuração global editável pela equipe, armazenada como chave/valor.
/// Hoje abriga apenas os dias de revisão (P13).
/// </summary>
public class Parametro
{
    /// <summary>Dias de revisão somados ao prazo de execução de todos os exames (Q1.6, P13).</summary>
    public const string DiasRevisao = "DiasRevisao";
    public const int DiasRevisaoPadrao = 3;

    public string Chave { get; private set; } = null!;
    public string Valor { get; private set; } = null!;
    public DateTime? AtualizadoEm { get; private set; }

    private Parametro() { } // EF Core

    public static Parametro CriarDiasRevisao(int dias = DiasRevisaoPadrao)
    {
        var p = new Parametro { Chave = DiasRevisao };
        p.AlterarDiasRevisao(dias);
        p.AtualizadoEm = null;
        return p;
    }

    public int ValorInteiro() => int.Parse(Valor, CultureInfo.InvariantCulture);

    public void AlterarDiasRevisao(int dias)
    {
        if (Chave != DiasRevisao)
            throw new InvalidOperationException($"O parâmetro '{Chave}' não é '{DiasRevisao}'.");
        if (dias < 0)
            throw new ValidacaoException("Os dias de revisão não podem ser negativos.");

        Valor = dias.ToString(CultureInfo.InvariantCulture);
        AtualizadoEm = DateTime.UtcNow;
    }
}

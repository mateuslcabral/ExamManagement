using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>Configuração global editável pela equipe (ex.: os 3 dias de revisão — P13/P28).</summary>
public class Parametro
{
    public const string ChaveDiasRevisao = "DiasRevisao";

    public string Chave { get; private set; } = null!;
    public string Valor { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public DateTime? AtualizadoEm { get; private set; }

    private Parametro() { } // EF Core

    public static Parametro Criar(string chave, string valor, string descricao)
        => new() { Chave = chave, Valor = valor, Descricao = descricao };

    public void AtualizarValor(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ValidacaoException("O valor do parâmetro é obrigatório.");
        Valor = valor.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public int ValorInteiro()
        => int.TryParse(Valor, out var v) ? v : throw new ValidacaoException($"O parâmetro '{Chave}' não é um número inteiro.");
}

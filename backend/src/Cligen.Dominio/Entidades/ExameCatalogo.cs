using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Exame do catálogo, mantido pela própria equipe (CRUD completo — Q3).
/// Nunca é apagado: exame aposentado é desativado, preservando o histórico dos exames solicitados.
/// </summary>
public class ExameCatalogo
{
    public const int TamanhoMaximoNome = 200;

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;

    /// <summary>
    /// Prazo de <b>execução</b> em dias corridos, fixo por exame (Q2, Q4).
    /// Não é o prazo até o paciente: esse soma os dias de revisão — ver <see cref="PrazoEntregaDias"/>.
    /// </summary>
    public int PrazoExecucaoDias { get; private set; }

    public decimal PrecoReferencia { get; private set; }
    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    private ExameCatalogo() { } // EF Core

    public static ExameCatalogo Criar(string nome, int prazoExecucaoDias, decimal precoReferencia)
    {
        var exame = new ExameCatalogo
        {
            Id = Guid.NewGuid(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
        exame.Aplicar(nome, prazoExecucaoDias, precoReferencia);
        return exame;
    }

    public void Atualizar(string nome, int prazoExecucaoDias, decimal precoReferencia)
    {
        Aplicar(nome, prazoExecucaoDias, precoReferencia);
        Tocar();
    }

    public void Desativar() { Ativo = false; Tocar(); }

    public void Reativar() { Ativo = true; Tocar(); }

    /// <summary>Previsão de entrega ao paciente: prazo de execução + dias de revisão (Q1.6, P13).</summary>
    public int PrazoEntregaDias(int diasRevisao) => PrazoExecucaoDias + diasRevisao;

    public static string NormalizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome do exame é obrigatório.");
        nome = nome.Trim();
        if (nome.Length > TamanhoMaximoNome)
            throw new ValidacaoException($"O nome do exame deve ter no máximo {TamanhoMaximoNome} caracteres.");
        return nome;
    }

    private void Aplicar(string nome, int prazoExecucaoDias, decimal precoReferencia)
    {
        nome = NormalizarNome(nome);
        if (prazoExecucaoDias <= 0)
            throw new ValidacaoException("O prazo de execução deve ser de pelo menos 1 dia.");
        if (precoReferencia < 0)
            throw new ValidacaoException("O preço de referência não pode ser negativo.");
        if (decimal.Round(precoReferencia, 2) != precoReferencia)
            throw new ValidacaoException("O preço de referência deve ter no máximo 2 casas decimais.");

        Nome = nome;
        PrazoExecucaoDias = prazoExecucaoDias;
        PrecoReferencia = precoReferencia;
    }

    private void Tocar() => AtualizadoEm = DateTime.UtcNow;
}

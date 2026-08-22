using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>Item do catálogo de exames (CLG-ESP §5). Prazo fixo por exame, em dias corridos (Q2, Q4).</summary>
public class ExameCatalogo
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;

    /// <summary>Prazo de execução — NÃO inclui os dias de revisão da Cligen (Q1.6).</summary>
    public int PrazoExecucaoDias { get; private set; }
    public decimal PrecoReferencia { get; private set; }

    /// <summary>Permite aposentar o exame sem apagá-lo do histórico.</summary>
    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    private ExameCatalogo() { } // EF Core

    public static ExameCatalogo Criar(string nome, int prazoExecucaoDias, decimal precoReferencia)
    {
        var e = new ExameCatalogo { Id = Guid.NewGuid(), Ativo = true, CriadoEm = DateTime.UtcNow };
        e.Aplicar(nome, prazoExecucaoDias, precoReferencia);
        return e;
    }

    public void Atualizar(string nome, int prazoExecucaoDias, decimal precoReferencia)
    {
        Aplicar(nome, prazoExecucaoDias, precoReferencia);
        AtualizadoEm = DateTime.UtcNow;
    }

    private void Aplicar(string nome, int prazoExecucaoDias, decimal precoReferencia)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome do exame é obrigatório.");
        if (prazoExecucaoDias <= 0)
            throw new ValidacaoException("O prazo de execução deve ser maior que zero.");
        if (precoReferencia < 0)
            throw new ValidacaoException("O preço de referência não pode ser negativo.");

        Nome = nome.Trim();
        PrazoExecucaoDias = prazoExecucaoDias;
        PrecoReferencia = decimal.Round(precoReferencia, 2);
    }

    /// <summary>Previsão de entrega ao paciente = execução + dias de revisão (parâmetro global, P13).</summary>
    public int PrazoTotalDias(int diasRevisao) => PrazoExecucaoDias + diasRevisao;

    public void Desativar() { Ativo = false; AtualizadoEm = DateTime.UtcNow; }
    public void Reativar() { Ativo = true; AtualizadoEm = DateTime.UtcNow; }
}

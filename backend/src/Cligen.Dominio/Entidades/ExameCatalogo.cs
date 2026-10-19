using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>Campos editáveis do item do catálogo (reunião de 17/10/2026).</summary>
/// <param name="PrazoDias">Prazo de entrega em dias corridos, já incluindo a revisão da Cligen — não há mais parâmetro de revisão.</param>
/// <param name="PrecoLimite">Piso: o valor a pagar de um exame (preço − desconto) não pode ficar abaixo dele.</param>
/// <param name="DestinoId">Laboratório padrão, pré-preenchido no exame e trocável.</param>
public sealed record DadosCatalogo(
    string? Nome,
    int PrazoDias,
    decimal PrecoReferencia,
    decimal PrecoLimite,
    int? NumeroGenes,
    string? Genes,
    Guid? DestinoId,
    string? InformacoesTecnicas);

/// <summary>
/// Exame do catálogo, mantido pela própria equipe (CRUD completo — Q3).
/// Nunca é apagado: exame aposentado é desativado, preservando o histórico dos exames solicitados.
/// </summary>
public class ExameCatalogo
{
    public const int TamanhoMaximoNome = 200;
    public const int TamanhoMaximoTexto = 4000;

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;

    /// <summary>Prazo de entrega ao paciente em dias corridos, contado do acolhimento (Q1, Q2). Inclui a revisão.</summary>
    public int PrazoDias { get; private set; }

    public decimal PrecoReferencia { get; private set; }

    /// <summary>Piso de preço: desconto não pode levar o valor a pagar abaixo dele.</summary>
    public decimal PrecoLimite { get; private set; }

    public int? NumeroGenes { get; private set; }

    /// <summary>Texto livre (lista de genes).</summary>
    public string? Genes { get; private set; }

    public Guid? DestinoId { get; private set; }
    public string? InformacoesTecnicas { get; private set; }

    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public Guid? CriadoPorId { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public Guid? AtualizadoPorId { get; private set; }

    private ExameCatalogo() { } // EF Core

    public static ExameCatalogo Criar(DadosCatalogo dados, Guid? autorId)
    {
        var exame = new ExameCatalogo
        {
            Id = Guid.NewGuid(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            CriadoPorId = autorId
        };
        exame.Aplicar(dados);
        return exame;
    }

    /// <summary>Atalho para seeds e testes: só nome, prazo e preço.</summary>
    public static ExameCatalogo Criar(string nome, int prazoDias, decimal precoReferencia, Guid? autorId = null)
        => Criar(new DadosCatalogo(nome, prazoDias, precoReferencia, 0m, null, null, null, null), autorId);

    public void Atualizar(DadosCatalogo dados, Guid autorId)
    {
        Aplicar(dados);
        Tocar(autorId);
    }

    public void Desativar(Guid autorId) { Ativo = false; Tocar(autorId); }

    public void Reativar(Guid autorId) { Ativo = true; Tocar(autorId); }

    public static string NormalizarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome do exame é obrigatório.");
        nome = nome.Trim();
        if (nome.Length > TamanhoMaximoNome)
            throw new ValidacaoException($"O nome do exame deve ter no máximo {TamanhoMaximoNome} caracteres.");
        return nome;
    }

    private void Aplicar(DadosCatalogo d)
    {
        var nome = NormalizarNome(d.Nome);
        if (d.PrazoDias <= 0)
            throw new ValidacaoException("O prazo deve ser de pelo menos 1 dia.");
        ValidarValor(d.PrecoReferencia, "O preço de referência");
        ValidarValor(d.PrecoLimite, "O preço limite");
        if (d.PrecoLimite > d.PrecoReferencia)
            throw new ValidacaoException("O preço limite não pode ser maior que o preço de referência.");
        if (d.NumeroGenes is < 0)
            throw new ValidacaoException("O número de genes não pode ser negativo.");

        Nome = nome;
        PrazoDias = d.PrazoDias;
        PrecoReferencia = d.PrecoReferencia;
        PrecoLimite = d.PrecoLimite;
        NumeroGenes = d.NumeroGenes;
        Genes = AparaTexto(d.Genes, "A lista de genes");
        DestinoId = d.DestinoId;
        InformacoesTecnicas = AparaTexto(d.InformacoesTecnicas, "As informações técnicas");
    }

    private static void ValidarValor(decimal valor, string rotulo)
    {
        if (valor < 0)
            throw new ValidacaoException($"{rotulo} não pode ser negativo.");
        if (decimal.Round(valor, 2) != valor)
            throw new ValidacaoException($"{rotulo} deve ter no máximo 2 casas decimais.");
    }

    private static string? AparaTexto(string? texto, string rotulo)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        texto = texto.Trim();
        if (texto.Length > TamanhoMaximoTexto)
            throw new ValidacaoException($"{rotulo} deve ter no máximo {TamanhoMaximoTexto} caracteres.");
        return texto;
    }

    private void Tocar(Guid autorId)
    {
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPorId = autorId;
    }
}

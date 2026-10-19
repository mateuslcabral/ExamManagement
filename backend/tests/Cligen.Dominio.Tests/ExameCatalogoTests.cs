using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class ExameCatalogoTests
{
    private static readonly Guid Autor = Guid.NewGuid();

    private static DadosCatalogo Dados(string nome = "Exoma", int prazo = 30, decimal referencia = 4500.50m, decimal limite = 0m,
        int? genes = null, string? listaGenes = null, Guid? destino = null, string? tecnicas = null)
        => new(nome, prazo, referencia, limite, genes, listaGenes, destino, tecnicas);

    [Fact]
    public void Criar_ComDadosValidos_DeveNascerAtivoComNomeAparadoEAutor()
    {
        var destino = Guid.NewGuid();
        var e = ExameCatalogo.Criar(Dados("  Sequenciamento Completo do Exoma ", 33, 4500.50m, 3000m, 20000, " BRCA1, BRCA2 ", destino, " NGS "), Autor);

        Assert.Equal("Sequenciamento Completo do Exoma", e.Nome);
        Assert.Equal(33, e.PrazoDias);
        Assert.Equal(4500.50m, e.PrecoReferencia);
        Assert.Equal(3000m, e.PrecoLimite);
        Assert.Equal(20000, e.NumeroGenes);
        Assert.Equal("BRCA1, BRCA2", e.Genes);
        Assert.Equal(destino, e.DestinoId);
        Assert.Equal("NGS", e.InformacoesTecnicas);
        Assert.True(e.Ativo);
        Assert.Equal(Autor, e.CriadoPorId);
        Assert.Null(e.AtualizadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemNome_DeveFalhar(string nome)
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(nome), Autor));

    [Fact]
    public void Criar_ComNomeLongoDemais_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(new string('a', ExameCatalogo.TamanhoMaximoNome + 1)), Autor));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Criar_ComPrazoNaoPositivo_DeveFalhar(int prazo)
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(prazo: prazo), Autor));

    [Fact]
    public void Criar_ComPrecoNegativoOuComMaisDeDuasCasas_DeveFalhar()
    {
        Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(referencia: -0.01m), Autor));
        Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(referencia: 10.005m), Autor));
        Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(limite: -1m), Autor));
    }

    [Fact]
    public void Criar_ComLimiteMaiorQueReferencia_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(referencia: 100m, limite: 100.01m), Autor));

    [Fact]
    public void Criar_ComNumeroDeGenesNegativo_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(Dados(genes: -1), Autor));

    [Fact]
    public void Criar_ComPrecoZero_DevePermitir()
        => Assert.Equal(0m, ExameCatalogo.Criar(Dados(referencia: 0m), Autor).PrecoReferencia);

    [Fact]
    public void Atualizar_DeveAlterarCamposEMarcarAutor()
    {
        var e = ExameCatalogo.Criar(Dados(), Autor);
        var outro = Guid.NewGuid();

        e.Atualizar(Dados("Exoma Clínico", 45, 200m, 150m), outro);

        Assert.Equal("Exoma Clínico", e.Nome);
        Assert.Equal(45, e.PrazoDias);
        Assert.Equal(200m, e.PrecoReferencia);
        Assert.Equal(150m, e.PrecoLimite);
        Assert.Equal(outro, e.AtualizadoPorId);
    }

    [Fact]
    public void Atualizar_ComDadoInvalido_NaoDeveAlterarNada()
    {
        var e = ExameCatalogo.Criar(Dados(), Autor);

        Assert.Throws<ValidacaoException>(() => e.Atualizar(Dados("Novo nome", 0, 200m), Autor));

        Assert.Equal("Exoma", e.Nome);
        Assert.Equal(4500.50m, e.PrecoReferencia);
    }

    [Fact]
    public void DesativarEReativar_DeveAlternarSituacao()
    {
        var e = ExameCatalogo.Criar(Dados(), Autor);

        e.Desativar(Autor);
        Assert.False(e.Ativo);

        e.Reativar(Autor);
        Assert.True(e.Ativo);
    }
}

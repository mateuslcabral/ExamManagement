using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class ExameCatalogoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveNascerAtivoComNomeAparado()
    {
        var e = ExameCatalogo.Criar("  Sequenciamento Completo do Exoma ", 30, 4500.50m);

        Assert.Equal("Sequenciamento Completo do Exoma", e.Nome);
        Assert.Equal(30, e.PrazoExecucaoDias);
        Assert.Equal(4500.50m, e.PrecoReferencia);
        Assert.True(e.Ativo);
        Assert.Null(e.AtualizadoEm);
    }

    [Fact]
    public void PrazoEntregaDias_DeveSomarDiasDeRevisaoAoPrazoDeExecucao()
    {
        // Q1.6: exame de 30 dias chega ao paciente em 33, não em 30.
        var e = ExameCatalogo.Criar("Exoma", 30, 100m);
        Assert.Equal(33, e.PrazoEntregaDias(3));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemNome_DeveFalhar(string nome)
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(nome, 30, 100m));

    [Fact]
    public void Criar_ComNomeLongoDemais_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() =>
            ExameCatalogo.Criar(new string('a', ExameCatalogo.TamanhoMaximoNome + 1), 30, 100m));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Criar_ComPrazoNaoPositivo_DeveFalhar(int prazo)
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar("Exoma", prazo, 100m));

    [Fact]
    public void Criar_ComPrecoNegativo_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar("Exoma", 30, -0.01m));

    [Fact]
    public void Criar_ComPrecoDeMaisDeDuasCasas_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar("Exoma", 30, 10.005m));

    [Fact]
    public void Criar_ComPrecoZero_DevePermitir()
        => Assert.Equal(0m, ExameCatalogo.Criar("Exoma", 30, 0m).PrecoReferencia);

    [Fact]
    public void Atualizar_DeveAlterarCamposEMarcarAtualizacao()
    {
        var e = ExameCatalogo.Criar("Exoma", 30, 100m);

        e.Atualizar("Exoma Clínico", 45, 200m);

        Assert.Equal("Exoma Clínico", e.Nome);
        Assert.Equal(45, e.PrazoExecucaoDias);
        Assert.Equal(200m, e.PrecoReferencia);
        Assert.NotNull(e.AtualizadoEm);
    }

    [Fact]
    public void Atualizar_ComDadoInvalido_NaoDeveAlterarNada()
    {
        var e = ExameCatalogo.Criar("Exoma", 30, 100m);

        Assert.Throws<ValidacaoException>(() => e.Atualizar("Novo nome", 0, 200m));

        Assert.Equal("Exoma", e.Nome);
        Assert.Equal(100m, e.PrecoReferencia);
    }

    [Fact]
    public void DesativarEReativar_DeveAlternarSituacao()
    {
        var e = ExameCatalogo.Criar("Exoma", 30, 100m);

        e.Desativar();
        Assert.False(e.Ativo);

        e.Reativar();
        Assert.True(e.Ativo);
    }
}

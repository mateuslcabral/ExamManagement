using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class AmostraTests
{
    private static readonly DateOnly Entrada = new(2026, 9, 10);
    private static readonly DateOnly Hoje = new(2026, 9, 17);
    private static readonly Guid Autor = Guid.NewGuid();

    private static (Exame Exame, ExameCatalogo Catalogo) NovoExame(int prazoExecucao = 30)
    {
        var catalogo = ExameCatalogo.Criar("Exoma", prazoExecucao, 100m);
        var exame = Exame.Criar(Guid.NewGuid(), catalogo, OrigemExame.Cligen, null, TipoMedico.Interno, null, null, Autor, Entrada);
        return (exame, catalogo);
    }

    [Fact]
    public void Acolher_DeveCalcularPrevisaoEmDiasCorridosEAvancarEstado()
    {
        var (exame, catalogo) = NovoExame(prazoExecucao: 30);

        exame.AcolherAmostra(new DateOnly(2026, 9, 15), catalogo, diasRevisao: 3, Autor, Hoje);

        // 15/09 + 30 + 3 = 18/10
        Assert.Equal(new DateOnly(2026, 10, 18), exame.DataLiberacaoPrevista);
        Assert.Equal(EstadoExame.AmostraAcolhida, exame.Estado);
        var amostra = Assert.Single(exame.Amostras);
        Assert.Equal(30, amostra.PrazoExecucaoDias);
        Assert.Equal(3, amostra.DiasRevisao);
        Assert.False(amostra.Recoleta);
        Assert.Equal(Autor, amostra.RegistradoPorId);
    }

    [Fact]
    public void Previsao_NaoMudaQuandoCatalogoOuRevisaoMudamDepois()
    {
        var (exame, catalogo) = NovoExame(prazoExecucao: 30);
        exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje);
        var prevista = exame.DataLiberacaoPrevista;

        catalogo.Atualizar("Exoma", 60, 100m); // P14: a data prometida é fixa

        Assert.Equal(prevista, exame.DataLiberacaoPrevista);
    }

    [Fact]
    public void Acolher_DuasVezes_DeveFalhar()
    {
        var (exame, catalogo) = NovoExame();
        exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje);

        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje));
    }

    [Fact]
    public void Acolher_ComDataFutura_DeveFalhar()
    {
        var (exame, catalogo) = NovoExame();
        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Hoje.AddDays(1), catalogo, 3, Autor, Hoje));
    }

    [Fact]
    public void Acolher_AntesDaEntradaDoExame_DeveFalhar()
    {
        var (exame, catalogo) = NovoExame();
        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Entrada.AddDays(-1), catalogo, 3, Autor, Hoje));
    }

    [Fact]
    public void Rejeitar_DeveZerarPrevisaoVoltarAguardandoEPreservarHistorico()
    {
        var (exame, catalogo) = NovoExame();
        exame.AcolherAmostra(Entrada, catalogo, 3, Autor, Hoje);

        exame.RejeitarAmostra(" Material hemolisado ", Autor);

        Assert.Null(exame.DataLiberacaoPrevista);
        Assert.Equal(EstadoExame.AguardandoAmostra, exame.Estado);
        Assert.Null(exame.AmostraVigente);
        var rejeitada = Assert.Single(exame.Amostras);
        Assert.Equal("Material hemolisado", rejeitada.MotivoRejeicao);
    }

    [Fact]
    public void Recoleta_DeveGerarNovaPrevisaoAPartirDoNovoAcolhimento()
    {
        var (exame, catalogo) = NovoExame(prazoExecucao: 10);
        exame.AcolherAmostra(Entrada, catalogo, 3, Autor, Hoje);
        exame.RejeitarAmostra("Volume insuficiente", Autor);

        exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje);

        Assert.Equal(Hoje.AddDays(13), exame.DataLiberacaoPrevista);
        Assert.Equal(2, exame.Amostras.Count);
        Assert.True(exame.AmostraVigente!.Recoleta);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Rejeitar_SemMotivo_DeveFalharSemAlterarOExame(string? motivo)
    {
        var (exame, catalogo) = NovoExame();
        exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje);

        Assert.Throws<ValidacaoException>(() => exame.RejeitarAmostra(motivo, Autor));

        Assert.Equal(EstadoExame.AmostraAcolhida, exame.Estado);
        Assert.NotNull(exame.DataLiberacaoPrevista);
    }

    [Fact]
    public void Rejeitar_SemAmostraAcolhida_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => NovoExame().Exame.RejeitarAmostra("motivo", Autor));

    [Fact]
    public void TrocarCatalogo_DepoisDoAcolhimento_DeveFalhar()
    {
        var (exame, catalogo) = NovoExame();
        exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje);

        Assert.Throws<ValidacaoException>(() => exame.Atualizar(
            ExameCatalogo.Criar("Painel", 5, 1m), OrigemExame.Cligen, null, TipoMedico.Interno, null, 100m, Autor));
    }

    [Fact]
    public void ExameExcluido_NaoPodeAcolher()
    {
        var (exame, catalogo) = NovoExame();
        exame.Excluir("teste", Autor);
        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje));
    }
}

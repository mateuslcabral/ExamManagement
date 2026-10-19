using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class AmostraTests
{
    private static readonly DateOnly Entrada = new(2026, 9, 10);
    private static readonly DateOnly Hoje = new(2026, 9, 17);
    private static readonly Guid Autor = Guid.NewGuid();

    private static (Exame Exame, ExameCatalogo Catalogo) NovoExame(int prazoDias = 30)
    {
        var catalogo = ExameCatalogo.Criar("Exoma", prazoDias, 100m);
        var exame = Exame.Criar(Guid.NewGuid(), catalogo,
            new DadosExame(Guid.NewGuid(), null, TipoMedico.Interno, null, null, null, null), Autor, Entrada);
        return (exame, catalogo);
    }

    [Fact]
    public void Acolher_DeveCalcularPrevisaoEmDiasCorridosEAvancarEstado()
    {
        var (exame, catalogo) = NovoExame(prazoDias: 33);

        exame.AcolherAmostra(new DateOnly(2026, 9, 15), catalogo, Autor, Hoje);

        // 15/09 + 33 = 18/10 — o prazo do catálogo já inclui a revisão
        Assert.Equal(new DateOnly(2026, 10, 18), exame.DataLiberacaoPrevista);
        Assert.Equal(new DateOnly(2026, 9, 15), exame.DataAcolhimento);
        Assert.Equal(EstadoExame.AmostraAcolhida, exame.Estado);
        var amostra = Assert.Single(exame.Amostras);
        Assert.Equal(33, amostra.PrazoDias);
        Assert.False(amostra.Recoleta);
        Assert.Equal(Autor, amostra.RegistradoPorId);
    }

    [Fact]
    public void Previsao_NaoMudaQuandoCatalogoMudaDepois()
    {
        var (exame, catalogo) = NovoExame(prazoDias: 30);
        exame.AcolherAmostra(Hoje, catalogo, Autor, Hoje);
        var prevista = exame.DataLiberacaoPrevista;

        catalogo.Atualizar(new DadosCatalogo("Exoma", 60, 100m, 0m, null, null, null, null), Autor); // P14: a data prometida é fixa

        Assert.Equal(prevista, exame.DataLiberacaoPrevista);
    }

    [Fact]
    public void Acolher_DuasVezes_DeveFalhar()
    {
        var (exame, catalogo) = NovoExame();
        exame.AcolherAmostra(Hoje, catalogo, Autor, Hoje);

        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Hoje, catalogo, Autor, Hoje));
    }

    [Fact]
    public void Acolher_ComDataFutura_DeveFalhar()
    {
        var (exame, catalogo) = NovoExame();
        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Hoje.AddDays(1), catalogo, Autor, Hoje));
    }

    [Fact]
    public void Acolher_AntesDaEntradaDoExame_DeveFalhar()
    {
        var (exame, catalogo) = NovoExame();
        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Entrada.AddDays(-1), catalogo, Autor, Hoje));
    }

    [Fact]
    public void Rejeitar_DeveZerarPrevisaoEAcolhimentoVoltarAguardandoEPreservarHistorico()
    {
        var (exame, catalogo) = NovoExame();
        exame.AcolherAmostra(Entrada, catalogo, Autor, Hoje);

        exame.RejeitarAmostra(" Material hemolisado ", Autor);

        Assert.Null(exame.DataLiberacaoPrevista);
        Assert.Null(exame.DataAcolhimento);
        Assert.Equal(EstadoExame.AguardandoAmostra, exame.Estado);
        Assert.Null(exame.AmostraVigente);
        var rejeitada = Assert.Single(exame.Amostras);
        Assert.Equal("Material hemolisado", rejeitada.MotivoRejeicao);
    }

    [Fact]
    public void Recoleta_DeveGerarNovaPrevisaoAPartirDoNovoAcolhimento()
    {
        var (exame, catalogo) = NovoExame(prazoDias: 10);
        exame.AcolherAmostra(Entrada, catalogo, Autor, Hoje);
        exame.RejeitarAmostra("Volume insuficiente", Autor);

        exame.AcolherAmostra(Hoje, catalogo, Autor, Hoje);

        Assert.Equal(Hoje.AddDays(10), exame.DataLiberacaoPrevista);
        Assert.Equal(2, exame.Amostras.Count);
        Assert.True(exame.AmostraVigente!.Recoleta);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Rejeitar_SemMotivo_DeveFalharSemAlterarOExame(string? motivo)
    {
        var (exame, catalogo) = NovoExame();
        exame.AcolherAmostra(Hoje, catalogo, Autor, Hoje);

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
        exame.AcolherAmostra(Hoje, catalogo, Autor, Hoje);

        Assert.Throws<ValidacaoException>(() => exame.Atualizar(
            ExameCatalogo.Criar("Painel", 5, 1m), new DadosExame(exame.OrigemId, null, TipoMedico.Interno, null, null, null, 100m), Autor));
    }

    [Fact]
    public void ExameExcluido_NaoPodeAcolher()
    {
        var (exame, catalogo) = NovoExame();
        exame.Excluir("teste", Autor);
        Assert.Throws<ValidacaoException>(() => exame.AcolherAmostra(Hoje, catalogo, Autor, Hoje));
    }
}

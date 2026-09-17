using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class FluxoLaudoTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 17);
    private static readonly Guid Autor = Guid.NewGuid();

    private static Exame ExameComAmostra()
    {
        var catalogo = ExameCatalogo.Criar("Exoma", 30, 100m);
        var exame = Exame.Criar(Guid.NewGuid(), catalogo, OrigemExame.Cligen, null, TipoMedico.Interno, null, null, Autor, Hoje);
        exame.AcolherAmostra(Hoje, catalogo, 3, Autor, Hoje);
        return exame;
    }

    private static ArquivoLaudo Arquivo(string nome = "laudo.pdf", string caminho = "2026/09/x.pdf")
        => new(nome, caminho, 1000, new string('a', 64));

    private static void Etapa(Exame e, TipoEtapaLaudo tipo) => e.RegistrarOuSubstituirEtapa(tipo, Arquivo(), Autor);

    [Fact]
    public void Etapas_SeguemAOrdemSemPular()
    {
        var e = ExameComAmostra();

        Assert.Throws<EstadoInvalidoException>(() => Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao)); // pulou a 3

        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Assert.Equal(EstadoExame.LaudoParceiroPronto, e.Estado);
        Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao);
        Etapa(e, TipoEtapaLaudo.LaudoRevisado);

        Assert.Equal(EstadoExame.LaudoRevisado, e.Estado);
        Assert.Equal(3, e.Etapas.Count);
        Assert.All(e.Etapas, et => Assert.Equal(Autor, et.RegistradoPorId));
    }

    [Fact]
    public void Etapa_SemAmostraAcolhida_DeveFalhar()
    {
        var catalogo = ExameCatalogo.Criar("Exoma", 30, 100m);
        var e = Exame.Criar(Guid.NewGuid(), catalogo, OrigemExame.Cligen, null, TipoMedico.Interno, null, null, Autor, Hoje);
        Assert.Throws<EstadoInvalidoException>(() => Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto));
    }

    [Fact]
    public void Substituir_NaoMudaEstadoNemDataEContaSubstituicoes()
    {
        var e = ExameComAmostra();
        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao);
        var data = e.Etapa(TipoEtapaLaudo.LaudoParceiroPronto)!.Data;
        var outro = Guid.NewGuid();

        var substituiu = e.RegistrarOuSubstituirEtapa(TipoEtapaLaudo.LaudoParceiroPronto, Arquivo("novo.pdf", "2026/09/novo.pdf"), outro);

        var etapa = e.Etapa(TipoEtapaLaudo.LaudoParceiroPronto)!;
        Assert.True(substituiu);
        Assert.Equal("novo.pdf", etapa.NomeOriginal);
        Assert.Equal("2026/09/novo.pdf", etapa.Caminho);
        Assert.Equal(data, etapa.Data);
        Assert.Equal(1, etapa.Substituicoes);
        Assert.Equal(outro, etapa.SubstituidoPorId);
        Assert.Equal(Autor, etapa.RegistradoPorId);
        Assert.Equal(EstadoExame.LaudoCligenParaRevisao, e.Estado);
    }

    [Fact]
    public void Rejeitar_AposInicioDoLaudo_DeveFalhar()
    {
        var e = ExameComAmostra();
        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Assert.Throws<ValidacaoException>(() => e.RejeitarAmostra("x", Autor));
    }

    [Fact]
    public void Disponibilizar_SoComLaudoRevisado_GravaDataEfetiva()
    {
        var e = ExameComAmostra();
        Assert.Throws<EstadoInvalidoException>(e.Disponibilizar);

        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao);
        Assert.Throws<EstadoInvalidoException>(e.Disponibilizar);

        Etapa(e, TipoEtapaLaudo.LaudoRevisado);
        e.Disponibilizar();

        Assert.Equal(EstadoExame.Disponibilizado, e.Estado);
        Assert.NotNull(e.DataLiberacaoEfetiva);
        Assert.NotNull(e.DataLiberacaoPrevista); // a prevista permanece, para medir o atraso
        Assert.Throws<EstadoInvalidoException>(e.Disponibilizar);
    }

    [Fact]
    public void Substituir_AposDisponibilizar_ContinuaPermitidoSemMudarEstado()
    {
        var e = ExameComAmostra();
        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao);
        Etapa(e, TipoEtapaLaudo.LaudoRevisado);
        e.Disponibilizar();

        e.RegistrarOuSubstituirEtapa(TipoEtapaLaudo.LaudoRevisado, Arquivo("corrigido.pdf"), Autor);

        Assert.Equal(EstadoExame.Disponibilizado, e.Estado);
        Assert.Equal(1, e.Etapa(TipoEtapaLaudo.LaudoRevisado)!.Substituicoes);
    }

    [Fact]
    public void ExameExcluido_NaoTransita()
    {
        var e = ExameComAmostra();
        e.Excluir("teste", Autor);
        Assert.Throws<ValidacaoException>(() => Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto));
        Assert.Throws<ValidacaoException>(e.Disponibilizar);
    }

    [Fact]
    public void Restaurar_DesfazAExclusaoERegistraAutor()
    {
        var e = ExameComAmostra();
        e.Excluir("engano", Autor);
        var outro = Guid.NewGuid();

        e.Restaurar(outro);

        Assert.False(e.Excluido);
        Assert.Null(e.MotivoExclusao);
        Assert.Equal(outro, e.AtualizadoPorId);
        Assert.Throws<ValidacaoException>(() => e.Restaurar(outro));
        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto); // volta a operar normalmente
    }
}

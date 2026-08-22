using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class FluxoLaudoTests
{
    private static readonly DateOnly Hoje = new(2026, 8, 22);
    private static readonly Guid Usuario = Guid.NewGuid();

    private static Exame NovoExame()
    {
        var p = Paciente.Criar("Maria", new DateOnly(1990, 1, 1), TipoDocumento.Cpf, "529.982.247-25", "m@x.com", "31999991234", null, Hoje);
        var c = ExameCatalogo.Criar("Exoma", 45, 6500m);
        return Exame.Criar(p, c, OrigemExame.Cligen, null, TipoMedicoSolicitante.Interno, null, null, Hoje);
    }

    private static void Etapa(Exame e, TipoEtapaLaudo tipo, string nome = "laudo.pdf")
        => e.RegistrarOuSubstituirEtapa(tipo, nome, $"l/{(int)tipo}.pdf", 100, "h", Usuario);

    [Fact]
    public void Acolher_CalculaPrevisaoUmaVez_EAvancaParaEstado2()
    {
        var e = NovoExame();
        var amanha = Hoje.AddDays(1); // exame entrou hoje; amostra chega amanhã

        e.AcolherAmostra(amanha, Usuario, 3, amanha);

        Assert.Equal(EstadoExame.AmostraAcolhida, e.Estado);
        Assert.Equal(amanha.AddDays(48), e.DataLiberacaoPrevista);
        Assert.NotNull(e.AmostraAtiva);
        Assert.Throws<EstadoInvalidoException>(() => e.AcolherAmostra(amanha, Usuario, 3, amanha));
    }

    [Fact]
    public void Acolher_DataInvalida_DeveFalhar()
    {
        var e = NovoExame();
        Assert.Throws<ValidacaoException>(() => e.AcolherAmostra(Hoje.AddDays(1), Usuario, 3, Hoje));
        Assert.Throws<ValidacaoException>(() => e.AcolherAmostra(Hoje.AddDays(-1), Usuario, 3, Hoje)); // antes da entrada
        Assert.Throws<ValidacaoException>(() => e.AcolherAmostra(Hoje, Guid.Empty, 3, Hoje));
    }

    [Fact]
    public void Rejeitar_ZeraPrazo_VoltaAoEstado1_ERecoletaRecalcula()
    {
        var e = NovoExame();
        e.AcolherAmostra(Hoje, Usuario, 3, Hoje);

        Assert.Throws<ValidacaoException>(() => e.RejeitarAmostra(" ", Usuario));
        e.RejeitarAmostra("Hemolisada", Usuario);

        Assert.Equal(EstadoExame.AguardandoAmostra, e.Estado);
        Assert.Null(e.DataLiberacaoPrevista);
        Assert.Null(e.AmostraAtiva);
        Assert.Single(e.Amostras);
        Assert.Equal(SituacaoAmostra.Rejeitada, e.Amostras.First().Situacao);
        Assert.Throws<EstadoInvalidoException>(() => e.RejeitarAmostra("de novo", Usuario));

        e.AcolherAmostra(Hoje, Usuario, 5, Hoje);
        Assert.Equal(2, e.Amostras.Count);
        Assert.Equal(Hoje.AddDays(50), e.DataLiberacaoPrevista);
    }

    [Fact]
    public void Rejeitar_AposInicioDoLaudo_NaoPermitido()
    {
        var e = NovoExame();
        e.AcolherAmostra(Hoje, Usuario, 3, Hoje);
        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Assert.Throws<EstadoInvalidoException>(() => e.RejeitarAmostra("x", Usuario));
    }

    [Fact]
    public void Etapas_SeguemOrdem_SemPular()
    {
        var e = NovoExame();
        Assert.Throws<EstadoInvalidoException>(() => Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto)); // sem amostra

        e.AcolherAmostra(Hoje, Usuario, 3, Hoje);
        Assert.Throws<EstadoInvalidoException>(() => Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao)); // pulou a 3

        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Assert.Equal(EstadoExame.LaudoParceiroPronto, e.Estado);
        Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao);
        Etapa(e, TipoEtapaLaudo.LaudoRevisado);
        Assert.Equal(EstadoExame.LaudoRevisado, e.Estado);
        Assert.Equal(3, e.Etapas.Count);
    }

    [Fact]
    public void Etapa_SomentePdf()
    {
        var e = NovoExame();
        e.AcolherAmostra(Hoje, Usuario, 3, Hoje);
        Assert.Throws<ValidacaoException>(() => Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto, "laudo.docx"));
        Assert.Throws<ValidacaoException>(() =>
            e.RegistrarOuSubstituirEtapa(TipoEtapaLaudo.LaudoParceiroPronto, "l.pdf", "c", EtapaAndamento.TamanhoMaximoBytes + 1, "h", Usuario));
    }

    [Fact]
    public void Substituir_NaoMudaEstadoNemData_EDevolveCaminhoAnterior()
    {
        var e = NovoExame();
        e.AcolherAmostra(Hoje, Usuario, 3, Hoje);
        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao);
        var data = e.Etapa(TipoEtapaLaudo.LaudoParceiroPronto)!.Data;

        var (etapa, anterior) = e.RegistrarOuSubstituirEtapa(TipoEtapaLaudo.LaudoParceiroPronto, "novo.pdf", "l/novo.pdf", 200, "h2", Usuario);

        Assert.Equal("l/3.pdf", anterior);
        Assert.Equal("novo.pdf", etapa.NomeOriginal);
        Assert.Equal(data, etapa.Data);
        Assert.Equal(1, etapa.Substituicoes);
        Assert.NotNull(etapa.ArquivoSubstituidoEm);
        Assert.Equal(EstadoExame.LaudoCligenParaRevisao, e.Estado);
    }

    [Fact]
    public void Disponibilizar_SoNoEstado5_GravaDataEfetiva()
    {
        var e = NovoExame();
        Assert.Throws<EstadoInvalidoException>(() => e.Disponibilizar());

        e.AcolherAmostra(Hoje, Usuario, 3, Hoje);
        Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto);
        Etapa(e, TipoEtapaLaudo.LaudoCligenParaRevisao);
        Assert.Throws<EstadoInvalidoException>(() => e.Disponibilizar());

        Etapa(e, TipoEtapaLaudo.LaudoRevisado);
        e.Disponibilizar();

        Assert.Equal(EstadoExame.DisponibilizadoAoPaciente, e.Estado);
        Assert.NotNull(e.DataLiberacaoEfetiva);
        Assert.True(e.Disponibilizado);
        Assert.Throws<EstadoInvalidoException>(() => e.Disponibilizar());

        // Substituição após liberação continua permitida (R2), sem mudar estado.
        e.RegistrarOuSubstituirEtapa(TipoEtapaLaudo.LaudoRevisado, "corrigido.pdf", "l/c.pdf", 100, "h3", Usuario);
        Assert.Equal(EstadoExame.DisponibilizadoAoPaciente, e.Estado);
    }

    [Fact]
    public void ExameExcluido_NaoTransita()
    {
        var e = NovoExame();
        e.Excluir(Usuario, "x");
        Assert.Throws<ExameExcluidoException>(() => e.AcolherAmostra(Hoje, Usuario, 3, Hoje));
        Assert.Throws<ExameExcluidoException>(() => Etapa(e, TipoEtapaLaudo.LaudoParceiroPronto));
        Assert.Throws<ExameExcluidoException>(() => e.Disponibilizar());
    }
}

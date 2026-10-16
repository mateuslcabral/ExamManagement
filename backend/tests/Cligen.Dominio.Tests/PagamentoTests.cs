using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class PagamentoTests
{
    private static readonly DateOnly Hoje = new(2026, 10, 16);
    private static readonly Guid Autor = Guid.NewGuid();
    private static readonly Guid ExameId = Guid.NewGuid();

    private static Pagamento Lancar(MetodoPagamento metodo, decimal valor = 100m, DateOnly? data = null,
        string? convenio = null, string? guia = null, int? parcelas = null, decimal? valorParcela = null)
        => Pagamento.Lancar(ExameId, metodo, valor, data, convenio, guia, parcelas, valorParcela, null, Autor, Hoje);

    [Fact]
    public void Particular_NasceRecebidoNaDataInformadaOuHoje()
    {
        var pix = Lancar(MetodoPagamento.Pix);
        Assert.Equal(StatusPagamento.Recebido, pix.Status);
        Assert.Equal(Hoje, pix.DataRecebimento);
        Assert.True(pix.ContaComoRecebido);

        var dinheiro = Lancar(MetodoPagamento.Dinheiro, data: Hoje.AddDays(-3));
        Assert.Equal(Hoje.AddDays(-3), dinheiro.DataRecebimento);
    }

    [Fact]
    public void Convenio_NasceAReceberSemData()
    {
        var p = Lancar(MetodoPagamento.Convenio, convenio: " Unimed ", guia: "G-123");

        Assert.Equal(StatusPagamento.AReceber, p.Status);
        Assert.Null(p.DataRecebimento);
        Assert.Equal("Unimed", p.Convenio);
        Assert.Equal("G-123", p.NumeroGuia);
        Assert.True(p.ContaComoAReceber);
        Assert.False(p.ContaComoRecebido);
    }

    [Fact]
    public void Convenio_SemNome_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Lancar(MetodoPagamento.Convenio));

    [Fact]
    public void Convenio_MarcarRecebido_EntraNoCaixaNaDataInformada()
    {
        var p = Lancar(MetodoPagamento.Convenio, convenio: "Unimed");
        p.MarcarRecebido(Hoje.AddDays(-1), Autor, Hoje);

        Assert.Equal(StatusPagamento.Recebido, p.Status);
        Assert.Equal(Hoje.AddDays(-1), p.DataRecebimento);
        Assert.Equal(Autor, p.AtualizadoPorId);
        Assert.Throws<ValidacaoException>(() => p.MarcarRecebido(Hoje, Autor, Hoje)); // já recebido
    }

    [Fact]
    public void Glosa_SoConvenioAReceber_ExigeMotivo()
    {
        var pix = Lancar(MetodoPagamento.Pix);
        Assert.Throws<ValidacaoException>(() => pix.Glosar("x", Autor));

        var conv = Lancar(MetodoPagamento.Convenio, convenio: "Unimed");
        Assert.Throws<ValidacaoException>(() => conv.Glosar(" ", Autor));
        conv.Glosar("Procedimento não coberto", Autor);

        Assert.Equal(StatusPagamento.Glosado, conv.Status);
        Assert.False(conv.ContaComoAReceber);
        Assert.False(conv.ContaComoRecebido);
        Assert.Throws<ValidacaoException>(() => conv.MarcarRecebido(Hoje, Autor, Hoje));
    }

    [Fact]
    public void DataRecebimentoFutura_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Lancar(MetodoPagamento.Pix, data: Hoje.AddDays(1)));

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(10.005)]
    public void ValorInvalido_DeveFalhar(double valor)
        => Assert.Throws<ValidacaoException>(() => Lancar(MetodoPagamento.Pix, (decimal)valor));

    [Fact]
    public void CartaoParcelado_GuardaParcelasECalculaValorQuandoNaoInformado()
    {
        var p = Lancar(MetodoPagamento.CartaoCredito, 1000m, parcelas: 3);
        Assert.Equal(3, p.Parcelas);
        Assert.Equal(333.33m, p.ValorParcela);
        Assert.Equal(StatusPagamento.Recebido, p.Status); // C6/P3: reconhecido integralmente na data

        var informado = Lancar(MetodoPagamento.CartaoCredito, 1000m, parcelas: 2, valorParcela: 510m);
        Assert.Equal(510m, informado.ValorParcela);
    }

    [Fact]
    public void Parcelas_IgnoradasForaDoCartaoDeCredito()
    {
        var p = Lancar(MetodoPagamento.Pix, 300m, parcelas: 3);
        Assert.Null(p.Parcelas);
    }

    [Fact]
    public void Excluir_ExigeMotivoETiraDosTotais()
    {
        var p = Lancar(MetodoPagamento.Pix);
        Assert.Throws<ValidacaoException>(() => p.Excluir("", Autor));

        p.Excluir("Lançado em duplicidade", Autor);

        Assert.True(p.Excluido);
        Assert.False(p.ContaComoRecebido);
        Assert.Throws<ValidacaoException>(() => p.Excluir("de novo", Autor));
    }

    [Fact]
    public void Desconto_EmReais_NaoPodeExcederOPreco()
    {
        var catalogo = ExameCatalogo.Criar("Exoma", 30, 1000m);
        var e = Exame.Criar(Guid.NewGuid(), catalogo, OrigemExame.Cligen, null, TipoMedico.Interno, null, null, Autor, Hoje);

        e.AplicarDesconto(150m, " Cortesia ", Autor);
        Assert.Equal(850m, e.ValorAPagar);
        Assert.Equal("Cortesia", e.MotivoDesconto);

        Assert.Throws<ValidacaoException>(() => e.AplicarDesconto(1000.01m, null, Autor));
        Assert.Throws<ValidacaoException>(() => e.AplicarDesconto(-1m, null, Autor));

        e.AplicarDesconto(0m, "ignorado", Autor);
        Assert.Equal(1000m, e.ValorAPagar);
        Assert.Null(e.MotivoDesconto);
    }
}

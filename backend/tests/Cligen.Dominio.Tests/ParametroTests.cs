using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class ParametroTests
{
    [Fact]
    public void CriarDiasRevisao_SemValor_DeveUsarPadraoDeTresDias()
    {
        var p = Parametro.CriarDiasRevisao();

        Assert.Equal(Parametro.DiasRevisao, p.Chave);
        Assert.Equal(3, p.ValorInteiro());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void AlterarDiasRevisao_ComValorValido_DeveGravar(int dias)
    {
        var p = Parametro.CriarDiasRevisao();

        p.AlterarDiasRevisao(dias);

        Assert.Equal(dias, p.ValorInteiro());
        Assert.NotNull(p.AtualizadoEm);
    }

    [Fact]
    public void AlterarDiasRevisao_Negativo_DeveFalhar()
    {
        var p = Parametro.CriarDiasRevisao();
        Assert.Throws<ValidacaoException>(() => p.AlterarDiasRevisao(-1));
        Assert.Equal(3, p.ValorInteiro());
    }
}

using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class CadastroSimplesTests
{
    private static readonly Guid Autor = Guid.NewGuid();

    [Fact]
    public void Criar_DeveNascerAtivoComNomeAparadoEAutor()
    {
        var o = Origem.Criar("  Clínica parceira ", Autor);

        Assert.Equal("Clínica parceira", o.Nome);
        Assert.True(o.Ativo);
        Assert.Equal(Autor, o.CriadoPorId);
        Assert.NotEqual(Guid.Empty, o.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Criar_SemNome_DeveFalhar(string? nome)
    {
        Assert.Throws<ValidacaoException>(() => Destino.Criar(nome, Autor));
        Assert.Throws<ValidacaoException>(() => Convenio.Criar(nome, Autor));
    }

    [Fact]
    public void Criar_ComNomeLongoDemais_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Convenio.Criar(new string('a', CadastroSimples.TamanhoMaximoNome + 1), Autor));

    [Fact]
    public void RenomearDesativarReativar_DevemRegistrarAutor()
    {
        var c = Convenio.Criar("Unimed", Autor);
        var outro = Guid.NewGuid();

        c.Renomear(" Unimed BH ", outro);
        Assert.Equal("Unimed BH", c.Nome);
        Assert.Equal(outro, c.AtualizadoPorId);

        c.Desativar(outro);
        Assert.False(c.Ativo);
        c.Reativar(outro);
        Assert.True(c.Ativo);
    }

    [Fact]
    public void Rotulos_SaoSingularesEmPortugues()
    {
        Assert.Equal("origem", Origem.Rotulo);
        Assert.Equal("destino", Destino.Rotulo);
        Assert.Equal("convênio", Convenio.Rotulo);
    }
}

using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class ExameTests
{
    private static readonly DateOnly Hoje = new(2026, 8, 22);

    private static Paciente PacienteAdulto() =>
        Paciente.Criar("Maria", new DateOnly(1990, 1, 1), TipoDocumento.Cpf, "529.982.247-25", "m@x.com", "31999991234", null, Hoje);

    private static ExameCatalogo Catalogo() => ExameCatalogo.Criar("Exoma", 45, 6500m);

    [Fact]
    public void Criar_HerdaPrecoDoCatalogo_ENasceAguardandoAmostra()
    {
        var e = Exame.Criar(PacienteAdulto(), Catalogo(), OrigemExame.Cligen, null, TipoMedicoSolicitante.Interno, null, null, Hoje);

        Assert.Equal(6500m, e.Preco);
        Assert.Equal(EstadoExame.AguardandoAmostra, e.Estado);
        Assert.Equal(Hoje, e.DataEntrada);
        Assert.Null(e.DataLiberacaoPrevista);
        Assert.Equal(Exame.NomeMedicoInterno, e.NomeMedico);
    }

    [Fact]
    public void Criar_PrecoInformado_SobrepoeCatalogo()
    {
        var e = Exame.Criar(PacienteAdulto(), Catalogo(), OrigemExame.SiteCligen, " Lab X ", TipoMedicoSolicitante.Externo, " Dra. Ana ", 5000.555m, Hoje);

        Assert.Equal(5000.56m, e.Preco);
        Assert.Equal("Lab X", e.Destino);
        Assert.Equal("Dra. Ana", e.NomeMedico);
    }

    [Fact]
    public void Criar_MedicoExternoSemNome_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() =>
            Exame.Criar(PacienteAdulto(), Catalogo(), OrigemExame.Cligen, null, TipoMedicoSolicitante.Externo, " ", null, Hoje));

    [Fact]
    public void Criar_CatalogoInativo_DeveFalhar()
    {
        var c = Catalogo();
        c.Desativar();
        Assert.Throws<ValidacaoException>(() =>
            Exame.Criar(PacienteAdulto(), c, OrigemExame.Cligen, null, TipoMedicoSolicitante.Interno, null, null, Hoje));
    }

    [Fact]
    public void Criar_PacienteExcluido_DeveFalhar()
    {
        var p = PacienteAdulto();
        p.Excluir(Guid.NewGuid(), "x");
        Assert.Throws<PacienteExcluidoException>(() =>
            Exame.Criar(p, Catalogo(), OrigemExame.Cligen, null, TipoMedicoSolicitante.Interno, null, null, Hoje));
    }

    [Fact]
    public void Anexos_LimiteDeTres_EFormatos()
    {
        var e = Exame.Criar(PacienteAdulto(), Catalogo(), OrigemExame.Cligen, null, TipoMedicoSolicitante.Interno, null, null, Hoje);

        e.AdicionarAnexo("pedido.pdf", "a/1.pdf", 100, "h1");
        e.AdicionarAnexo("foto.JPG", "a/2.jpg", 100, "h2");
        Assert.Equal("image/jpeg", e.Anexos.Last().TipoConteudo);
        Assert.Throws<ValidacaoException>(() => e.AdicionarAnexo("virus.exe", "a/3.exe", 100, "h3"));
        Assert.Throws<ValidacaoException>(() => e.AdicionarAnexo("grande.pdf", "a/3.pdf", Anexo.TamanhoMaximoBytes + 1, "h3"));
        e.AdicionarAnexo("laudo.png", "a/3.png", 100, "h3");
        Assert.Throws<LimiteDeAnexosException>(() => e.AdicionarAnexo("extra.pdf", "a/4.pdf", 100, "h4"));

        var removido = e.RemoverAnexo(e.Anexos.First().Id);
        Assert.Equal("pedido.pdf", removido.NomeOriginal);
        Assert.Equal(2, e.Anexos.Count);
        Assert.Throws<RegistroNaoEncontradoException>(() => e.RemoverAnexo(Guid.NewGuid()));
    }

    [Fact]
    public void Excluir_ERestaurar()
    {
        var e = Exame.Criar(PacienteAdulto(), Catalogo(), OrigemExame.Cligen, null, TipoMedicoSolicitante.Interno, null, null, Hoje);
        var autor = Guid.NewGuid();

        Assert.Throws<ValidacaoException>(() => e.Excluir(autor, ""));
        e.Excluir(autor, "Duplicado");
        Assert.True(e.Excluido);
        Assert.Throws<ExameExcluidoException>(() => e.Atualizar(OrigemExame.Cligen, null, TipoMedicoSolicitante.Interno, null, 1));
        Assert.Throws<ExameExcluidoException>(() => e.AdicionarAnexo("a.pdf", "a", 1, "h"));

        e.Restaurar();
        Assert.False(e.Excluido);
        Assert.Null(e.MotivoExclusao);
    }
}

public class ExameCatalogoTests
{
    [Fact]
    public void Criar_ValidaEArredonda()
    {
        var c = ExameCatalogo.Criar("  Exoma ", 30, 1234.567m);
        Assert.Equal("Exoma", c.Nome);
        Assert.Equal(1234.57m, c.PrecoReferencia);
        Assert.True(c.Ativo);
        Assert.Equal(33, c.PrazoTotalDias(3));
    }

    [Theory]
    [InlineData("", 10, 1)]
    [InlineData("Exoma", 0, 1)]
    [InlineData("Exoma", 10, -1)]
    public void Criar_Invalido_DeveFalhar(string nome, int prazo, decimal preco)
        => Assert.Throws<ValidacaoException>(() => ExameCatalogo.Criar(nome, prazo, preco));
}

public class ParametroTests
{
    [Fact]
    public void ValorInteiro_EAtualizacao()
    {
        var p = Parametro.Criar(Parametro.ChaveDiasRevisao, "3", "d");
        Assert.Equal(3, p.ValorInteiro());
        p.AtualizarValor(" 5 ");
        Assert.Equal(5, p.ValorInteiro());
        p.AtualizarValor("x");
        Assert.Throws<ValidacaoException>(() => p.ValorInteiro());
        Assert.Throws<ValidacaoException>(() => p.AtualizarValor(" "));
    }
}

using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;
using Cligen.Dominio.ValueObjects;

namespace Cligen.Dominio.Tests;

public class PacienteTests
{
    private static readonly DateOnly Hoje = new(2026, 8, 22);
    private const string CpfValido = "529.982.247-25";

    private static Paciente Adulto() =>
        Paciente.Criar("Maria Silva", new DateOnly(1990, 5, 10), TipoDocumento.Cpf, CpfValido,
            "Maria@Exemplo.com", "(31) 99999-1234", null, Hoje);

    private static Paciente.DadosResponsavel Responsavel() =>
        new("João Silva", TipoDocumento.Cpf, "111.444.777-35", "Pai");

    [Fact]
    public void Criar_Adulto_NormalizaDocumentoEmailTelefone()
    {
        var p = Adulto();

        Assert.Equal("52998224725", p.NumeroDocumento);
        Assert.Equal("maria@exemplo.com", p.Email);
        Assert.Equal("31999991234", p.Telefone);
        Assert.Null(p.ResponsavelLegal);
        Assert.False(p.Excluido);
        Assert.Equal(36, p.Idade(Hoje));
    }

    [Fact]
    public void Criar_MenorSemResponsavel_DeveFalhar()
    {
        Assert.Throws<ResponsavelLegalObrigatorioException>(() =>
            Paciente.Criar("Ana", new DateOnly(2010, 1, 1), TipoDocumento.Cpf, CpfValido,
                "pai@exemplo.com", "31999991234", null, Hoje));
    }

    [Fact]
    public void Criar_MenorComResponsavel_VinculaResponsavel()
    {
        var p = Paciente.Criar("Ana", new DateOnly(2010, 1, 1), TipoDocumento.Cpf, CpfValido,
            "pai@exemplo.com", "31999991234", Responsavel(), Hoje);

        Assert.True(p.EhMenorDeIdade(Hoje));
        Assert.NotNull(p.ResponsavelLegal);
        Assert.Equal("11144477735", p.ResponsavelLegal!.NumeroDocumento);
        Assert.Equal(p.Id, p.ResponsavelLegal.PacienteId);
    }

    [Fact]
    public void Maioridade_ContaAniversarioExato()
    {
        var faz18Hoje = Paciente.Criar("Ana", Hoje.AddYears(-18), TipoDocumento.Cpf, CpfValido, "a@b.com", "31999991234", null, Hoje);
        Assert.False(faz18Hoje.EhMenorDeIdade(Hoje));

        Assert.Throws<ResponsavelLegalObrigatorioException>(() =>
            Paciente.Criar("Ana", Hoje.AddYears(-18).AddDays(1), TipoDocumento.Cpf, CpfValido, "a@b.com", "31999991234", null, Hoje));
    }

    [Fact]
    public void Atualizar_QuandoViraAdulto_DescartaResponsavel()
    {
        var p = Paciente.Criar("Ana", new DateOnly(2010, 1, 1), TipoDocumento.Cpf, CpfValido,
            "pai@exemplo.com", "31999991234", Responsavel(), Hoje);

        p.Atualizar("Ana", new DateOnly(2000, 1, 1), TipoDocumento.Cpf, CpfValido, "ana@exemplo.com", "31999991234", Responsavel(), Hoje);

        Assert.Null(p.ResponsavelLegal);
        Assert.NotNull(p.AtualizadoEm);
    }

    [Fact]
    public void Criar_DataNascimentoFutura_DeveFalhar()
    {
        Assert.Throws<ValidacaoException>(() =>
            Paciente.Criar("Ana", Hoje.AddDays(1), TipoDocumento.Cpf, CpfValido, "a@b.com", "31999991234", null, Hoje));
    }

    [Fact]
    public void Excluir_ExigeMotivoEAutor_EBloqueiaEdicao()
    {
        var p = Adulto();
        var autor = Guid.NewGuid();

        Assert.Throws<ValidacaoException>(() => p.Excluir(autor, " "));
        Assert.Throws<ValidacaoException>(() => p.Excluir(Guid.Empty, "Duplicado"));

        p.Excluir(autor, "Cadastro duplicado");

        Assert.True(p.Excluido);
        Assert.Equal(autor, p.ExcluidoPorUsuarioId);
        Assert.Equal("Cadastro duplicado", p.MotivoExclusao);
        Assert.Throws<PacienteExcluidoException>(() =>
            p.Atualizar("Outro", p.DataNascimento, p.TipoDocumento, p.NumeroDocumento, p.Email, p.Telefone, null, Hoje));
        Assert.Throws<PacienteExcluidoException>(() => p.Excluir(autor, "De novo"));
    }

    [Fact]
    public void Restaurar_LimpaExclusao()
    {
        var p = Adulto();
        Assert.Throws<ValidacaoException>(() => p.Restaurar());

        p.Excluir(Guid.NewGuid(), "Engano");
        p.Restaurar();

        Assert.False(p.Excluido);
        Assert.Null(p.MotivoExclusao);
        Assert.Null(p.ExcluidoPorUsuarioId);
    }
}

public class DocumentoTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("11144477735", "11144477735")]
    public void Cpf_Valido_RetornaSoDigitos(string entrada, string esperado)
        => Assert.Equal(esperado, Documento.Normalizar(TipoDocumento.Cpf, entrada));

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-26")]
    [InlineData("1234")]
    [InlineData("")]
    public void Cpf_Invalido_DeveFalhar(string entrada)
        => Assert.Throws<ValidacaoException>(() => Documento.Normalizar(TipoDocumento.Cpf, entrada));

    [Fact]
    public void Passaporte_NormalizaMaiusculas()
        => Assert.Equal("AB123456", Documento.Normalizar(TipoDocumento.Passaporte, " ab-123456 "));

    [Theory]
    [InlineData("AB1")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    public void Passaporte_TamanhoInvalido_DeveFalhar(string entrada)
        => Assert.Throws<ValidacaoException>(() => Documento.Normalizar(TipoDocumento.Passaporte, entrada));

    [Fact]
    public void Formatar_Cpf()
        => Assert.Equal("529.982.247-25", Documento.Formatar(TipoDocumento.Cpf, "52998224725"));
}

public class TelefoneTests
{
    [Theory]
    [InlineData("(31) 99999-1234", "31999991234")]
    [InlineData("+55 31 99999-1234", "5531999991234")]
    [InlineData("3133334444", "3133334444")]
    public void Normaliza_SoDigitos(string entrada, string esperado)
        => Assert.Equal(esperado, Telefone.Normalizar(entrada));

    [Theory]
    [InlineData("999991234")]
    [InlineData("55 31 99999 1234 99")]
    [InlineData("")]
    public void Invalido_DeveFalhar(string entrada)
        => Assert.Throws<ValidacaoException>(() => Telefone.Normalizar(entrada));
}

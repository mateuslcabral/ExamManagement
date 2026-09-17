using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class PacienteTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 16);
    private static readonly Guid Autor = Guid.NewGuid();
    private static readonly Documento CpfPaciente = Documento.Criar(TipoDocumento.Cpf, "529.982.247-25");
    private static readonly Documento CpfResponsavel = Documento.Criar(TipoDocumento.Cpf, "111.444.777-35");

    private static ResponsavelLegal Responsavel() => ResponsavelLegal.Criar("Maria Souza", CpfResponsavel, "Mãe");

    private static Paciente CriarAdulto() => Paciente.Criar(
        " João Souza ", new DateOnly(1990, 5, 10), CpfPaciente, "Joao@Email.com", "(31) 99999-8888", null, Autor, Hoje);

    [Fact]
    public void Criar_Adulto_DeveNormalizarContatoERegistrarAutor()
    {
        var p = CriarAdulto();

        Assert.Equal("João Souza", p.Nome);
        Assert.Equal("joao@email.com", p.Email);
        Assert.Equal("+5531999998888", p.Telefone);
        Assert.Equal("52998224725", p.NumeroDocumento);
        Assert.Equal(Autor, p.CriadoPorId);
        Assert.False(p.EhMenorDeIdade(Hoje));
        Assert.Null(p.ResponsavelLegal);
    }

    [Fact]
    public void Criar_MenorSemResponsavel_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Paciente.Criar(
            "Ana", new DateOnly(2015, 1, 1), CpfPaciente, "mae@email.com", "31999998888", null, Autor, Hoje));

    [Fact]
    public void Criar_MenorComResponsavel_DevePermitir()
    {
        var p = Paciente.Criar("Ana", new DateOnly(2015, 1, 1), CpfPaciente, "mae@email.com", "31999998888", Responsavel(), Autor, Hoje);

        Assert.True(p.EhMenorDeIdade(Hoje));
        Assert.Equal("Maria Souza", p.ResponsavelLegal!.Nome);
    }

    [Fact]
    public void Criar_AdultoComResponsavel_DevePermitir()
    {
        // P9: paciente sem contato próprio usa o do responsável.
        var p = Paciente.Criar("José", new DateOnly(1940, 1, 1), CpfPaciente, "filha@email.com", "31999998888", Responsavel(), Autor, Hoje);
        Assert.NotNull(p.ResponsavelLegal);
    }

    [Theory]
    [InlineData(2008, 9, 16, false)] // faz 18 hoje
    [InlineData(2008, 9, 17, true)]  // faz 18 amanhã
    public void EhMenorDeIdade_DeveConsiderarDiaDoAniversario(int ano, int mes, int dia, bool menor)
        => Assert.Equal(menor, Paciente.IdadeEm(new DateOnly(ano, mes, dia), Hoje) < Paciente.MaioridadeAnos);

    [Fact]
    public void Criar_ResponsavelComMesmoDocumentoDoPaciente_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Paciente.Criar(
            "Ana", new DateOnly(2015, 1, 1), CpfPaciente, "mae@email.com", "31999998888",
            ResponsavelLegal.Criar("Maria", CpfPaciente, null), Autor, Hoje));

    [Theory]
    [InlineData(null, "31999998888")]
    [InlineData("joao@email.com", null)]
    public void Criar_SemEmailOuTelefone_DeveFalhar(string? email, string? telefone)
        => Assert.Throws<ValidacaoException>(() => Paciente.Criar(
            "João", new DateOnly(1990, 1, 1), CpfPaciente, email, telefone, null, Autor, Hoje));

    [Fact]
    public void Criar_ComNascimentoNoFuturo_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Paciente.Criar(
            "João", Hoje.AddDays(1), CpfPaciente, "joao@email.com", "31999998888", Responsavel(), Autor, Hoje));

    [Fact]
    public void Atualizar_DeveRegistrarAutorDaAlteracaoEPermitirRemoverResponsavelDeAdulto()
    {
        var p = Paciente.Criar("José", new DateOnly(1940, 1, 1), CpfPaciente, "filha@email.com", "31999998888", Responsavel(), Autor, Hoje);
        var outroAutor = Guid.NewGuid();

        p.Atualizar("José", new DateOnly(1940, 1, 1), CpfPaciente, "jose@email.com", "31988887777", null, outroAutor, Hoje);

        Assert.Null(p.ResponsavelLegal);
        Assert.Equal(outroAutor, p.AtualizadoPorId);
        Assert.Equal(Autor, p.CriadoPorId);
        Assert.NotNull(p.AtualizadoEm);
    }

    [Fact]
    public void ResponsavelLegal_SemNome_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => ResponsavelLegal.Criar(" ", CpfResponsavel, null));
}

using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class ExameTests
{
    private static readonly DateOnly Hoje = new(2026, 9, 16);
    private static readonly Guid Autor = Guid.NewGuid();
    private static readonly Guid PacienteId = Guid.NewGuid();

    private static ExameCatalogo Catalogo(string nome = "Exoma", decimal preco = 4500m) => ExameCatalogo.Criar(nome, 30, preco);

    private static Exame CriarExame(ExameCatalogo? catalogo = null, decimal? preco = null) => Exame.Criar(
        PacienteId, catalogo ?? Catalogo(), OrigemExame.Cligen, " Laboratório X ", TipoMedico.Interno, null, preco, Autor, Hoje);

    private static Anexo NovoAnexo() => Anexo.Criar("laudo.pdf", FormatoArquivo.Pdf, 1000, new string('a', 64), "2026/09/x.pdf", Autor);

    [Fact]
    public void Criar_DeveNascerAguardandoAmostraComPrecoDoCatalogoEDataDeEntrada()
    {
        var e = CriarExame();

        Assert.Equal(EstadoExame.AguardandoAmostra, e.Estado);
        Assert.Equal(4500m, e.Preco);
        Assert.Equal(Hoje, e.DataEntrada);
        Assert.Equal("Laboratório X", e.Destino);
        Assert.Equal(Exame.NomeMedicoInterno, e.NomeMedico);
        Assert.Equal(Autor, e.CriadoPorId);
    }

    [Fact]
    public void Criar_ComPrecoInformado_DeveSobreporOPrecoDoCatalogo()
        => Assert.Equal(3999.90m, CriarExame(preco: 3999.90m).Preco);

    [Fact]
    public void Criar_ComCatalogoInativo_DeveFalhar()
    {
        var catalogo = Catalogo();
        catalogo.Desativar();
        Assert.Throws<ValidacaoException>(() => CriarExame(catalogo));
    }

    [Fact]
    public void Criar_MedicoExternoSemNome_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Exame.Criar(
            PacienteId, Catalogo(), OrigemExame.ClinicaParceira, null, TipoMedico.Externo, "  ", null, Autor, Hoje));

    [Fact]
    public void Criar_MedicoInterno_DeveDescartarNomeExterno()
    {
        var e = Exame.Criar(PacienteId, Catalogo(), OrigemExame.Cligen, null, TipoMedico.Interno, "Dr. Fulano", null, Autor, Hoje);
        Assert.Null(e.NomeMedicoExterno);
    }

    [Fact]
    public void Atualizar_ComDadoInvalido_NaoDeveTrocarOCatalogo()
    {
        var e = CriarExame();
        var original = e.ExameCatalogoId;

        Assert.Throws<ValidacaoException>(() =>
            e.Atualizar(Catalogo("Painel"), OrigemExame.Cligen, null, TipoMedico.Externo, null, 100m, Autor));

        Assert.Equal(original, e.ExameCatalogoId);
    }

    [Fact]
    public void Atualizar_DeveRegistrarAutor()
    {
        var e = CriarExame();
        var outro = Guid.NewGuid();
        var novoCatalogo = Catalogo("Painel");

        e.Atualizar(novoCatalogo, OrigemExame.Plataforma, null, TipoMedico.Externo, "Dra. Ana", 10m, outro);

        Assert.Equal(novoCatalogo.Id, e.ExameCatalogoId);
        Assert.Equal("Dra. Ana", e.NomeMedico);
        Assert.Equal(outro, e.AtualizadoPorId);
    }

    [Fact]
    public void AdicionarAnexo_AlemDeTres_DeveFalhar()
    {
        var e = CriarExame();
        for (var i = 0; i < Exame.MaximoAnexos; i++) e.AdicionarAnexo(NovoAnexo());

        Assert.Throws<ValidacaoException>(() => e.AdicionarAnexo(NovoAnexo()));
    }

    [Fact]
    public void RemoverAnexo_DeveLiberarVagaSemApagarORegistro()
    {
        var e = CriarExame();
        var anexos = Enumerable.Range(0, Exame.MaximoAnexos).Select(_ => NovoAnexo()).ToList();
        anexos.ForEach(e.AdicionarAnexo);

        e.RemoverAnexo(anexos[0].Id, Autor);
        e.AdicionarAnexo(NovoAnexo());

        Assert.Equal(4, e.Anexos.Count);
        Assert.False(anexos[0].Ativo);
        Assert.Equal(Autor, anexos[0].RemovidoPorId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Excluir_SemMotivo_DeveFalhar(string? motivo)
        => Assert.Throws<ValidacaoException>(() => CriarExame().Excluir(motivo, Autor));

    [Fact]
    public void Excluir_DeveGuardarAutorMotivoEBloquearAlteracoes()
    {
        var e = CriarExame();

        e.Excluir(" Lançado no paciente errado ", Autor);

        Assert.True(e.Excluido);
        Assert.Equal("Lançado no paciente errado", e.MotivoExclusao);
        Assert.Equal(Autor, e.ExcluidoPorId);
        Assert.Throws<ValidacaoException>(() => e.AdicionarAnexo(NovoAnexo()));
        Assert.Throws<ValidacaoException>(() => e.Excluir("de novo", Autor));
    }

    [Theory]
    [InlineData(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37 }, ".pdf")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ".png")]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 }, ".jpg")]
    public void FormatoArquivo_DeveDetectarPelosPrimeirosBytes(byte[] cabecalho, string extensao)
        => Assert.Equal(extensao, FormatoArquivo.Detectar(cabecalho, 100).Extensao);

    [Fact]
    public void FormatoArquivo_ComOutroFormato_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => FormatoArquivo.Detectar("PK\u0003\u0004zip!"u8, 100));

    [Fact]
    public void FormatoArquivo_AcimaDe50MB_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => FormatoArquivo.Detectar("%PDF-1.7"u8, FormatoArquivo.TamanhoMaximoBytes + 1));

    [Fact]
    public void Anexo_NomeComCaminho_DeveGuardarSoONomeDoArquivo()
        => Assert.Equal("laudo.pdf", Anexo.Criar(@"C:\fakepath\laudo.pdf", FormatoArquivo.Pdf, 10, new string('a', 64), "x", Autor).NomeOriginal);
}

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
    private static readonly Guid OrigemId = Guid.NewGuid();
    private static readonly Guid DestinoCatalogo = Guid.NewGuid();

    private static ExameCatalogo Catalogo(string nome = "Exoma", decimal preco = 4500m, decimal limite = 0m)
        => ExameCatalogo.Criar(new DadosCatalogo(nome, 30, preco, limite, null, null, DestinoCatalogo, null), Autor);

    private static DadosExame Dados(decimal? preco = null, Guid? destino = null, TipoMedico tipo = TipoMedico.Interno,
        string? nome = null, string? crm = null, string? uf = null)
        => new(OrigemId, destino, tipo, nome, crm, uf, preco);

    private static Exame CriarExame(ExameCatalogo? catalogo = null, decimal? preco = null)
        => Exame.Criar(PacienteId, catalogo ?? Catalogo(), Dados(preco), Autor, Hoje);

    private static Anexo NovoAnexo(TipoAnexo tipo = TipoAnexo.Outro)
        => Anexo.Criar(tipo, "laudo.pdf", FormatoArquivo.Pdf, 1000, new string('a', 64), "2026/09/x.pdf", Autor);

    [Fact]
    public void Criar_DeveNascerAguardandoAmostraComPrecoEDestinoDoCatalogo()
    {
        var e = CriarExame();

        Assert.Equal(EstadoExame.AguardandoAmostra, e.Estado);
        Assert.Equal(4500m, e.Preco);
        Assert.Equal(DestinoCatalogo, e.DestinoId);
        Assert.Equal(OrigemId, e.OrigemId);
        Assert.Equal(Hoje, e.DataEntrada);
        Assert.Equal(Exame.NomeMedicoInterno, e.NomeMedico);
        Assert.Equal(Autor, e.CriadoPorId);
        Assert.Null(e.DataAcolhimento);
    }

    [Fact]
    public void Codigo_DerivaDoNumeroSequencial()
    {
        var e = CriarExame();
        Assert.Equal("EX-000000", e.Codigo); // Numero é atribuído pelo banco
    }

    [Fact]
    public void Criar_ComPrecoEDestinoInformados_DeveSobreporOCatalogo()
    {
        var outroDestino = Guid.NewGuid();
        var e = Exame.Criar(PacienteId, Catalogo(), Dados(3999.90m, outroDestino), Autor, Hoje);
        Assert.Equal(3999.90m, e.Preco);
        Assert.Equal(outroDestino, e.DestinoId);
    }

    [Fact]
    public void Criar_SemOrigem_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Exame.Criar(
            PacienteId, Catalogo(), new DadosExame(Guid.Empty, null, TipoMedico.Interno, null, null, null, null), Autor, Hoje));

    [Fact]
    public void Criar_ComCatalogoInativo_DeveFalhar()
    {
        var catalogo = Catalogo();
        catalogo.Desativar(Autor);
        Assert.Throws<ValidacaoException>(() => CriarExame(catalogo));
    }

    [Fact]
    public void Criar_AbaixoDoPrecoLimite_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => CriarExame(Catalogo(limite: 1000m), preco: 999.99m));

    [Fact]
    public void MedicoExterno_ExigeNomeEValidaUf()
    {
        Assert.Throws<ValidacaoException>(() => Exame.Criar(PacienteId, Catalogo(), Dados(tipo: TipoMedico.Externo, nome: " "), Autor, Hoje));
        Assert.Throws<ValidacaoException>(() => Exame.Criar(PacienteId, Catalogo(), Dados(tipo: TipoMedico.Externo, nome: "Dra. Ana", uf: "XX"), Autor, Hoje));

        var e = Exame.Criar(PacienteId, Catalogo(), Dados(tipo: TipoMedico.Externo, nome: "Dra. Ana", crm: " 12345 ", uf: "mg"), Autor, Hoje);

        Assert.Equal("Dra. Ana", e.NomeMedico);
        Assert.Equal("12345", e.CrmMedico);
        Assert.Equal("MG", e.UfCrm);
    }

    [Fact]
    public void MedicoInterno_DescartaNomeCrmEUfExternos()
    {
        var e = Exame.Criar(PacienteId, Catalogo(), Dados(tipo: TipoMedico.Interno, nome: "Dr. Fulano", crm: "1", uf: "SP"), Autor, Hoje);
        Assert.Null(e.NomeMedicoExterno);
        Assert.Null(e.CrmMedico);
        Assert.Null(e.UfCrm);
    }

    [Fact]
    public void Atualizar_ComDadoInvalido_NaoDeveTrocarOCatalogo()
    {
        var e = CriarExame();
        var original = e.ExameCatalogoId;

        Assert.Throws<ValidacaoException>(() =>
            e.Atualizar(Catalogo("Painel"), Dados(100m, tipo: TipoMedico.Externo, nome: null), Autor));

        Assert.Equal(original, e.ExameCatalogoId);
    }

    [Fact]
    public void Atualizar_SemPreco_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => CriarExame().Atualizar(Catalogo(), Dados(preco: null), Autor));

    [Fact]
    public void Atualizar_DeveRegistrarAutor()
    {
        var e = CriarExame();
        var outro = Guid.NewGuid();
        var novoCatalogo = Catalogo("Painel");

        e.Atualizar(novoCatalogo, Dados(10m, tipo: TipoMedico.Externo, nome: "Dra. Ana"), outro);

        Assert.Equal(novoCatalogo.Id, e.ExameCatalogoId);
        Assert.Equal("Dra. Ana", e.NomeMedico);
        Assert.Equal(outro, e.AtualizadoPorId);
    }

    [Fact]
    public void Atualizar_PrecoAbaixoDoLimiteConsiderandoDesconto_DeveFalhar()
    {
        var catalogo = Catalogo(preco: 1000m, limite: 800m);
        var e = CriarExame(catalogo);
        e.AplicarDesconto(100m, null, catalogo.PrecoLimite, Autor);

        Assert.Throws<ValidacaoException>(() => e.Atualizar(catalogo, Dados(850m), Autor)); // 850 − 100 < 800
        e.Atualizar(catalogo, Dados(900m), Autor); // 900 − 100 = 800, no limite
        Assert.Equal(900m, e.Preco);
    }

    [Fact]
    public void AdicionarAnexo_AlemDeTres_DeveFalhar()
    {
        var e = CriarExame();
        for (var i = 0; i < Exame.MaximoAnexos; i++) e.AdicionarAnexo(NovoAnexo());

        Assert.Throws<ValidacaoException>(() => e.AdicionarAnexo(NovoAnexo()));
    }

    [Fact]
    public void Anexo_GuardaOTipo_ERecusaTipoInvalido()
    {
        Assert.Equal(TipoAnexo.TermoConsentimento, NovoAnexo(TipoAnexo.TermoConsentimento).Tipo);
        Assert.Throws<ValidacaoException>(() => NovoAnexo((TipoAnexo)99));
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

    [Fact]
    public void Comentario_RegistraAutorEData_ERecusaVazio()
    {
        var e = CriarExame();
        var c = e.AdicionarComentario("  Paciente ligou perguntando do prazo. ", Autor);

        Assert.Single(e.Comentarios);
        Assert.Equal("Paciente ligou perguntando do prazo.", c.Texto);
        Assert.Equal(Autor, c.AutorId);
        Assert.Throws<ValidacaoException>(() => e.AdicionarComentario("   ", Autor));
        Assert.Throws<ValidacaoException>(() => e.AdicionarComentario(new string('x', Comentario.TamanhoMaximoTexto + 1), Autor));
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
        Assert.Throws<ValidacaoException>(() => e.AdicionarComentario("x", Autor));
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
        => Assert.Equal("laudo.pdf", Anexo.Criar(TipoAnexo.PedidoMedico, @"C:\fakepath\laudo.pdf", FormatoArquivo.Pdf, 10, new string('a', 64), "x", Autor).NomeOriginal);
}

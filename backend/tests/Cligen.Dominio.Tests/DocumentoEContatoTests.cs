using Cligen.Dominio.Comum;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class DocumentoEContatoTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    [InlineData(" 111.444.777-35 ", "11144477735")]
    public void Cpf_Valido_DeveSerNormalizadoParaDigitos(string entrada, string esperado)
        => Assert.Equal(esperado, Documento.Criar(TipoDocumento.Cpf, entrada).Numero);

    [Theory]
    [InlineData("529.982.247-26")] // dígito verificador errado
    [InlineData("111.111.111-11")] // todos iguais passam no cálculo, mas são inválidos
    [InlineData("1234567890")]
    [InlineData("")]
    public void Cpf_Invalido_DeveFalhar(string entrada)
        => Assert.Throws<ValidacaoException>(() => Documento.Criar(TipoDocumento.Cpf, entrada));

    [Theory]
    [InlineData("fz 123-456", "FZ123456")]
    [InlineData("AB12345", "AB12345")]
    public void Passaporte_Valido_DeveSerNormalizado(string entrada, string esperado)
        => Assert.Equal(esperado, Documento.Criar(TipoDocumento.Passaporte, entrada).Numero);

    [Theory]
    [InlineData("AB1")]
    [InlineData("AB12#45")]
    public void Passaporte_Invalido_DeveFalhar(string entrada)
        => Assert.Throws<ValidacaoException>(() => Documento.Criar(TipoDocumento.Passaporte, entrada));

    [Fact]
    public void Documento_MesmoTipoENumero_DeveSerIgual()
        => Assert.Equal(Documento.Criar(TipoDocumento.Cpf, "529.982.247-25"), Documento.Criar(TipoDocumento.Cpf, "52998224725"));

    [Theory]
    [InlineData("(31) 99999-8888", "+5531999998888")]
    [InlineData("31 3333-4444", "+553133334444")]
    [InlineData("+55 31 99999-8888", "+5531999998888")]
    [InlineData("5531999998888", "+5531999998888")]
    [InlineData("+1 (415) 555-2671", "+14155552671")]
    public void Telefone_Valido_DeveVirarFormatoInternacional(string entrada, string esperado)
        => Assert.Equal(esperado, Contato.NormalizarTelefone(entrada));

    [Theory]
    [InlineData("99999-8888")]     // sem DDD
    [InlineData("14155552671")]    // estrangeiro sem "+"
    [InlineData("")]
    public void Telefone_Invalido_DeveFalhar(string entrada)
        => Assert.Throws<ValidacaoException>(() => Contato.NormalizarTelefone(entrada));
}

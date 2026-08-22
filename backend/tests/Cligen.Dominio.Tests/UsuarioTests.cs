using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Tests;

public class UsuarioTests
{
    [Theory]
    [InlineData("ana@gmail.com")]
    [InlineData("  Ana.Silva@GMAIL.COM ")]
    public void Criar_ComEmailGmail_DeveSerGoogleEAtivoNaCriacao(string email)
    {
        var u = Usuario.Criar("Ana", email);

        Assert.Equal(TipoLogin.Google, u.TipoLogin);
        Assert.True(u.Ativo);
        Assert.Null(u.SenhaHash);
        Assert.Equal(email.Trim().ToLowerInvariant(), u.Email);
    }

    [Theory]
    [InlineData("ana@cligen.com.br")]
    [InlineData("ana@outlook.com")]
    [InlineData("ana@notgmail.com")]
    public void Criar_ComOutroEmail_DeveSerLocalEInativoAteDefinirSenha(string email)
    {
        var u = Usuario.Criar("Ana", email);

        Assert.Equal(TipoLogin.Local, u.TipoLogin);
        Assert.False(u.Ativo);
        Assert.False(u.PodeAutenticarComSenha());
    }

    [Fact]
    public void DefinirSenha_EmContaLocal_DeveAtivar()
    {
        var u = Usuario.Criar("Ana", "ana@cligen.com.br");

        u.DefinirSenha("hash");

        Assert.True(u.Ativo);
        Assert.True(u.PodeAutenticarComSenha());
    }

    [Fact]
    public void DefinirSenha_EmContaGoogle_DeveFalhar()
    {
        var u = Usuario.Criar("Ana", "ana@gmail.com");
        Assert.Throws<OperacaoInvalidaParaTipoLoginException>(() => u.DefinirSenha("hash"));
    }

    [Fact]
    public void VincularGoogle_EmContaLocal_DeveFalhar()
    {
        var u = Usuario.Criar("Ana", "ana@cligen.com.br");
        Assert.Throws<OperacaoInvalidaParaTipoLoginException>(() => u.VincularGoogle("sub"));
    }

    [Fact]
    public void Reativar_ContaLocalSemSenha_DeveFalhar()
    {
        var u = Usuario.Criar("Ana", "ana@cligen.com.br");
        Assert.Throws<ValidacaoException>(u.Reativar);
    }

    [Fact]
    public void Desativar_DeveBloquearAutenticacao()
    {
        var u = Usuario.Criar("Ana", "ana@cligen.com.br");
        u.DefinirSenha("hash");
        u.Desativar();
        Assert.False(u.PodeAutenticarComSenha());
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    [InlineData("sem@dominio")]
    public void Criar_ComEmailInvalido_DeveFalhar(string email)
        => Assert.Throws<ValidacaoException>(() => Usuario.Criar("Ana", email));

    [Fact]
    public void Criar_SemNome_DeveFalhar()
        => Assert.Throws<ValidacaoException>(() => Usuario.Criar("  ", "ana@gmail.com"));
}

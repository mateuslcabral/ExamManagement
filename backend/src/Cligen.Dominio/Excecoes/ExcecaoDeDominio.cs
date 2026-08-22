namespace Cligen.Dominio.Excecoes;

/// <summary>Base para toda violação de regra de negócio. A Api traduz para HTTP 4xx.</summary>
public abstract class ExcecaoDeDominio(string mensagem) : Exception(mensagem);

public sealed class EmailJaCadastradoException(string email)
    : ExcecaoDeDominio($"Já existe um usuário cadastrado com o e-mail '{email}'.");

public sealed class UsuarioInativoException()
    : ExcecaoDeDominio("Este usuário está inativo e não pode acessar o sistema.");

public sealed class OperacaoInvalidaParaTipoLoginException(string operacao)
    : ExcecaoDeDominio($"A operação '{operacao}' não se aplica a este tipo de login.");

public sealed class ValidacaoException(string mensagem) : ExcecaoDeDominio(mensagem);

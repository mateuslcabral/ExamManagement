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

// ---- Paciente ----

/// <summary>Mensagem fixada pela especificação (Q11, Q12), válida para qualquer tipo de documento.</summary>
public sealed class DocumentoJaCadastradoException()
    : ExcecaoDeDominio("Esse CPF já foi utilizado");

public sealed class ResponsavelLegalObrigatorioException()
    : ExcecaoDeDominio("Paciente menor de idade: informe o responsável legal.");

public sealed class PacienteExcluidoException()
    : ExcecaoDeDominio("Este paciente está excluído. Restaure o cadastro antes de alterá-lo.");

public sealed class RegistroNaoEncontradoException(string entidade)
    : ExcecaoDeDominio($"{entidade} não encontrado.");

// ---- Exame ----

public sealed class ExameExcluidoException()
    : ExcecaoDeDominio("Este exame está excluído. Restaure-o antes de alterá-lo.");

public sealed class LimiteDeAnexosException()
    : ExcecaoDeDominio("O exame já possui o máximo de 3 anexos.");

public sealed class PacienteComExamesAtivosException()
    : ExcecaoDeDominio("Este paciente possui exames ativos e não pode ser excluído.");

public sealed class NomeDeExameJaCadastradoException(string nome)
    : ExcecaoDeDominio($"Já existe um exame no catálogo com o nome '{nome}'.");

// ---- Amostra e laudo ----

/// <summary>Transição não permitida pela matriz de estados (P32).</summary>
public sealed class EstadoInvalidoException(string operacao, string estadoAtual)
    : ExcecaoDeDominio($"Não é possível {operacao}: o exame está em '{estadoAtual}'.");

public sealed class AmostraNaoAcolhidaException()
    : ExcecaoDeDominio("O exame não possui amostra acolhida.");

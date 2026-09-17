using Cligen.Dominio.Comum;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Cadastro único e persistente do paciente (D2), feito só pela equipe (D1). A chave é um GUID sem relação com o
/// documento (C1); o documento tem índice único e é o login do portal. Não há exclusão: é prontuário (Q45).
/// </summary>
public class Paciente
{
    public const int TamanhoMaximoNome = 200;
    public const int MaioridadeAnos = 18;
    private static readonly DateOnly NascimentoMinimo = new(1900, 1, 1);

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public DateOnly DataNascimento { get; private set; }
    public TipoDocumento TipoDocumento { get; private set; }
    public string NumeroDocumento { get; private set; } = null!;

    /// <summary>Destino dos disparos. Para menor, é o e-mail do responsável (P9).</summary>
    public string Email { get; private set; } = null!;

    /// <summary>Formato internacional (+55...), destino do WhatsApp. Para menor, é o do responsável (P9).</summary>
    public string Telefone { get; private set; } = null!;

    public ResponsavelLegal? ResponsavelLegal { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public Guid CriadoPorId { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public Guid? AtualizadoPorId { get; private set; }

    private Paciente() { } // EF Core

    public Documento Documento => Documento.Criar(TipoDocumento, NumeroDocumento);

    /// <param name="hoje">Data local da clínica, usada para decidir a menoridade.</param>
    public static Paciente Criar(
        string? nome, DateOnly dataNascimento, Documento documento, string? email, string? telefone,
        ResponsavelLegal? responsavel, Guid autorId, DateOnly hoje)
    {
        var paciente = new Paciente
        {
            Id = Guid.NewGuid(),
            CriadoEm = DateTime.UtcNow,
            CriadoPorId = autorId
        };
        paciente.Aplicar(nome, dataNascimento, documento, email, telefone, responsavel, hoje);
        return paciente;
    }

    public void Atualizar(
        string? nome, DateOnly dataNascimento, Documento documento, string? email, string? telefone,
        ResponsavelLegal? responsavel, Guid autorId, DateOnly hoje)
    {
        Aplicar(nome, dataNascimento, documento, email, telefone, responsavel, hoje);
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPorId = autorId;
    }

    public bool EhMenorDeIdade(DateOnly hoje) => IdadeEm(DataNascimento, hoje) < MaioridadeAnos;

    public static int IdadeEm(DateOnly nascimento, DateOnly hoje)
    {
        var idade = hoje.Year - nascimento.Year;
        return nascimento > hoje.AddYears(-idade) ? idade - 1 : idade;
    }

    private void Aplicar(
        string? nome, DateOnly dataNascimento, Documento documento, string? email, string? telefone,
        ResponsavelLegal? responsavel, DateOnly hoje)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome do paciente é obrigatório.");
        nome = nome.Trim();
        if (nome.Length > TamanhoMaximoNome)
            throw new ValidacaoException($"O nome deve ter no máximo {TamanhoMaximoNome} caracteres.");

        if (dataNascimento > hoje)
            throw new ValidacaoException("A data de nascimento não pode estar no futuro.");
        if (dataNascimento < NascimentoMinimo)
            throw new ValidacaoException("Data de nascimento inválida.");

        // Sem e-mail e telefone não há acesso ao portal, portanto não há cadastro (Q10).
        var emailNormalizado = Contato.NormalizarEmail(email);
        var telefoneNormalizado = Contato.NormalizarTelefone(telefone);

        // Obrigatório para menor (Q7). Para maior é opcional: cobre o paciente sem contato próprio (P9).
        if (responsavel is null && IdadeEm(dataNascimento, hoje) < MaioridadeAnos)
            throw new ValidacaoException("Paciente menor de idade precisa de responsável legal.");
        if (responsavel is not null && responsavel.Documento == documento)
            throw new ValidacaoException("O documento do responsável legal não pode ser o mesmo do paciente.");

        Nome = nome;
        DataNascimento = dataNascimento;
        TipoDocumento = documento.Tipo;
        NumeroDocumento = documento.Numero;
        Email = emailNormalizado;
        Telefone = telefoneNormalizado;
        ResponsavelLegal = responsavel;
    }
}

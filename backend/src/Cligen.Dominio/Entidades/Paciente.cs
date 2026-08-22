using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;
using Cligen.Dominio.ValueObjects;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Cadastro único e persistente do paciente (D1, D2). Só a equipe Cligen cria; exames posteriores
/// se vinculam ao mesmo registro. Chave GUID sem relação com o documento (C1).
/// </summary>
public class Paciente
{
    public const int IdadeMaioridade = 18;

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public DateOnly DataNascimento { get; private set; }
    public TipoDocumento TipoDocumento { get; private set; }
    public string NumeroDocumento { get; private set; } = null!;

    /// <summary>Destino dos disparos. Para menor de idade é o contato do responsável (P9).</summary>
    public string Email { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;

    public ResponsavelLegal? ResponsavelLegal { get; private set; }

    // Exclusão lógica (P16): natureza de prontuário, nunca apaga fisicamente.
    public DateTime? ExcluidoEm { get; private set; }
    public Guid? ExcluidoPorUsuarioId { get; private set; }
    public string? MotivoExclusao { get; private set; }
    public bool Excluido => ExcluidoEm is not null;

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    private Paciente() { } // EF Core

    public sealed record DadosResponsavel(string Nome, TipoDocumento TipoDocumento, string NumeroDocumento, string? Parentesco);

    public static Paciente Criar(
        string nome,
        DateOnly dataNascimento,
        TipoDocumento tipoDocumento,
        string numeroDocumento,
        string email,
        string telefone,
        DadosResponsavel? responsavel,
        DateOnly hoje)
    {
        var p = new Paciente { Id = Guid.NewGuid(), CriadoEm = DateTime.UtcNow };
        p.AplicarDados(nome, dataNascimento, tipoDocumento, numeroDocumento, email, telefone, responsavel, hoje);
        return p;
    }

    public void Atualizar(
        string nome,
        DateOnly dataNascimento,
        TipoDocumento tipoDocumento,
        string numeroDocumento,
        string email,
        string telefone,
        DadosResponsavel? responsavel,
        DateOnly hoje)
    {
        GarantirNaoExcluido();
        AplicarDados(nome, dataNascimento, tipoDocumento, numeroDocumento, email, telefone, responsavel, hoje);
        AtualizadoEm = DateTime.UtcNow;
    }

    private void AplicarDados(
        string nome,
        DateOnly dataNascimento,
        TipoDocumento tipoDocumento,
        string numeroDocumento,
        string email,
        string telefone,
        DadosResponsavel? responsavel,
        DateOnly hoje)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome é obrigatório.");
        if (dataNascimento > hoje)
            throw new ValidacaoException("A data de nascimento não pode ser futura.");
        if (dataNascimento < hoje.AddYears(-130))
            throw new ValidacaoException("A data de nascimento não é válida.");

        Nome = nome.Trim();
        DataNascimento = dataNascimento;
        TipoDocumento = tipoDocumento;
        NumeroDocumento = Documento.Normalizar(tipoDocumento, numeroDocumento);
        Email = Usuario.NormalizarEmail(email);
        Telefone = ValueObjects.Telefone.Normalizar(telefone);

        if (EhMenorDeIdade(hoje))
        {
            if (responsavel is null)
                throw new ResponsavelLegalObrigatorioException();

            if (ResponsavelLegal is null)
                ResponsavelLegal = ResponsavelLegal.Criar(Id, responsavel.Nome, responsavel.TipoDocumento, responsavel.NumeroDocumento, responsavel.Parentesco);
            else
                ResponsavelLegal.Atualizar(responsavel.Nome, responsavel.TipoDocumento, responsavel.NumeroDocumento, responsavel.Parentesco);
        }
        else
        {
            // Adulto: responsável é descartado mesmo que enviado — o contato é do próprio paciente.
            ResponsavelLegal = null;
        }
    }

    public int Idade(DateOnly hoje)
    {
        var idade = hoje.Year - DataNascimento.Year;
        if (DataNascimento > hoje.AddYears(-idade)) idade--;
        return idade;
    }

    public bool EhMenorDeIdade(DateOnly hoje) => Idade(hoje) < IdadeMaioridade;

    public void Excluir(Guid usuarioId, string motivo)
    {
        GarantirNaoExcluido();
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ValidacaoException("O motivo da exclusão é obrigatório.");
        if (usuarioId == Guid.Empty)
            throw new ValidacaoException("Autor da exclusão não identificado.");

        ExcluidoEm = DateTime.UtcNow;
        ExcluidoPorUsuarioId = usuarioId;
        MotivoExclusao = motivo.Trim();
    }

    public void Restaurar()
    {
        if (!Excluido)
            throw new ValidacaoException("Este paciente não está excluído.");
        ExcluidoEm = null;
        ExcluidoPorUsuarioId = null;
        MotivoExclusao = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    private void GarantirNaoExcluido()
    {
        if (Excluido) throw new PacienteExcluidoException();
    }
}

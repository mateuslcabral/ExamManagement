using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Acolhimento de amostra no laboratório, acessado sempre através de <see cref="Exame"/>. Tabela própria, e não
/// campos do exame, para absorver Q1.4/Q1.5 sem migration destrutiva; hoje vale um acolhimento vigente por exame (P8).
/// Amostra rejeitada permanece como histórico; a seguinte é marcada como recoleta (Q19).
/// </summary>
public class Amostra
{
    public const int TamanhoMaximoMotivoRejeicao = 500;

    public Guid Id { get; private set; }

    /// <summary>Data em que a amostra chegou ao laboratório — início da contagem do prazo (Q1).</summary>
    public DateOnly DataAcolhimento { get; private set; }

    /// <summary>Prazo do catálogo e dias de revisão vigentes no acolhimento: explicam a data prevista gravada.</summary>
    public int PrazoExecucaoDias { get; private set; }
    public int DiasRevisao { get; private set; }

    public DateOnly DataLiberacaoPrevista { get; private set; }
    public bool Recoleta { get; private set; }

    /// <summary>Quando e por quem o acolhimento foi lançado — distinto de <see cref="DataAcolhimento"/>, que é digitada.</summary>
    public DateTime RegistradoEm { get; private set; }
    public Guid RegistradoPorId { get; private set; }

    public DateTime? RejeitadaEm { get; private set; }
    public Guid? RejeitadaPorId { get; private set; }
    public string? MotivoRejeicao { get; private set; }

    public bool Rejeitada => RejeitadaEm is not null;

    private Amostra() { } // EF Core

    internal static Amostra Acolher(DateOnly dataAcolhimento, int prazoExecucaoDias, int diasRevisao, bool recoleta, Guid autorId)
        => new()
        {
            Id = Guid.NewGuid(),
            DataAcolhimento = dataAcolhimento,
            PrazoExecucaoDias = prazoExecucaoDias,
            DiasRevisao = diasRevisao,
            // Dias corridos, sem horário de corte (P7).
            DataLiberacaoPrevista = dataAcolhimento.AddDays(prazoExecucaoDias + diasRevisao),
            Recoleta = recoleta,
            RegistradoEm = DateTime.UtcNow,
            RegistradoPorId = autorId
        };

    internal void Rejeitar(string? motivo, Guid autorId)
    {
        if (Rejeitada) throw new ValidacaoException("Esta amostra já foi rejeitada.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ValidacaoException("Informe o motivo da rejeição da amostra.");
        motivo = motivo.Trim();
        if (motivo.Length > TamanhoMaximoMotivoRejeicao)
            throw new ValidacaoException($"O motivo deve ter no máximo {TamanhoMaximoMotivoRejeicao} caracteres.");

        RejeitadaEm = DateTime.UtcNow;
        RejeitadaPorId = autorId;
        MotivoRejeicao = motivo;
    }
}

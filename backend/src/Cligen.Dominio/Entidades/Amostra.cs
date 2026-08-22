using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Acolhimento de amostra (CLG-ESP §7). Histórico 1-N por exame; só uma fica ativa (P30).
/// A rejeição preserva o registro e a recoleta gera um novo (P31).
/// </summary>
public class Amostra
{
    public Guid Id { get; private set; }
    public Guid ExameId { get; private set; }
    public DateOnly DataAcolhimento { get; private set; }
    public SituacaoAmostra Situacao { get; private set; }
    public Guid RegistradoPorUsuarioId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public string? MotivoRejeicao { get; private set; }
    public DateTime? RejeitadaEm { get; private set; }
    public Guid? RejeitadaPorUsuarioId { get; private set; }

    public bool Ativa => Situacao == SituacaoAmostra.Acolhida;

    private Amostra() { } // EF Core

    internal static Amostra Acolher(Guid exameId, DateOnly dataAcolhimento, Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new ValidacaoException("Autor do acolhimento não identificado.");

        return new Amostra
        {
            Id = Guid.NewGuid(),
            ExameId = exameId,
            DataAcolhimento = dataAcolhimento,
            Situacao = SituacaoAmostra.Acolhida,
            RegistradoPorUsuarioId = usuarioId,
            CriadoEm = DateTime.UtcNow
        };
    }

    internal void Rejeitar(string motivo, Guid usuarioId)
    {
        if (!Ativa)
            throw new ValidacaoException("Esta amostra já foi rejeitada.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ValidacaoException("O motivo da rejeição é obrigatório.");
        if (usuarioId == Guid.Empty)
            throw new ValidacaoException("Autor da rejeição não identificado.");

        Situacao = SituacaoAmostra.Rejeitada;
        MotivoRejeicao = motivo.Trim();
        RejeitadaEm = DateTime.UtcNow;
        RejeitadaPorUsuarioId = usuarioId;
    }
}

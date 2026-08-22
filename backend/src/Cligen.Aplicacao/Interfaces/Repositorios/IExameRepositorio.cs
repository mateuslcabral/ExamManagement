using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

public interface IExameRepositorio
{
    /// <summary>Agregado completo (paciente, catálogo, anexos), excluído ou não.</summary>
    Task<Exame?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Exame>> ListarAsync(string? busca, bool incluirExcluidos, CancellationToken ct = default);
    Task<IReadOnlyList<Exame>> ListarPorPacienteAsync(Guid pacienteId, bool incluirExcluidos, CancellationToken ct = default);
    Task<bool> PacientePossuiExamesAtivosAsync(Guid pacienteId, CancellationToken ct = default);
    Task AdicionarAsync(Exame exame, CancellationToken ct = default);
    Task AtualizarAsync(Exame exame, CancellationToken ct = default);
}

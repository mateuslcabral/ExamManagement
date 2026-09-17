using Cligen.Aplicacao.DTOs;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

/// <summary>Exames excluídos logicamente não aparecem em nenhuma consulta daqui (saem das telas — C2).</summary>
public interface IExameRepositorio
{
    Task<Exame?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <param name="busca">Parte do nome do paciente ou do exame, ou início do documento do paciente.</param>
    Task<(IReadOnlyList<ExameResumoDto> Itens, int Total)> BuscarAsync(
        string? busca, Guid? pacienteId, EstadoExame? estado, int pagina, int tamanhoPagina, CancellationToken ct = default);

    Task AdicionarAsync(Exame exame, CancellationToken ct = default);
    Task AtualizarAsync(Exame exame, CancellationToken ct = default);
}

using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

public interface IPacienteRepositorio
{
    Task<Paciente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteDocumentoAsync(Documento documento, Guid? ignorarId = null, CancellationToken ct = default);

    /// <param name="busca">Parte do nome ou início do número do documento. Vazio lista todos.</param>
    Task<(IReadOnlyList<Paciente> Itens, int Total)> BuscarAsync(string? busca, int pagina, int tamanhoPagina, CancellationToken ct = default);

    Task AdicionarAsync(Paciente paciente, CancellationToken ct = default);
    Task AtualizarAsync(Paciente paciente, CancellationToken ct = default);
}

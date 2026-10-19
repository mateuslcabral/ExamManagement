using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string>> ObterNomesAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken ct = default);
}

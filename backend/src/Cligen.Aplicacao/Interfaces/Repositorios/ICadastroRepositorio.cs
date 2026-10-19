using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

/// <summary>Repositório dos cadastros auxiliares (origem, destino, convênio). Uma implementação genérica serve os três.</summary>
public interface ICadastroRepositorio<T> where T : CadastroSimples
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteNomeAsync(string nome, Guid? ignorarId = null, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default);
    Task AdicionarAsync(T item, CancellationToken ct = default);
    Task AtualizarAsync(T item, CancellationToken ct = default);
}

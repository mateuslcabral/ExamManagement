using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

public interface IExameCatalogoRepositorio
{
    Task<ExameCatalogo?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteNomeAsync(string nome, Guid? ignorarId = null, CancellationToken ct = default);
    Task<IReadOnlyList<ExameCatalogo>> ListarAsync(bool somenteAtivos, CancellationToken ct = default);
    Task AdicionarAsync(ExameCatalogo item, CancellationToken ct = default);
    Task AtualizarAsync(ExameCatalogo item, CancellationToken ct = default);
}

public interface IParametroRepositorio
{
    Task<Parametro?> ObterAsync(string chave, CancellationToken ct = default);
    Task<IReadOnlyList<Parametro>> ListarAsync(CancellationToken ct = default);
    Task AtualizarAsync(Parametro parametro, CancellationToken ct = default);
}

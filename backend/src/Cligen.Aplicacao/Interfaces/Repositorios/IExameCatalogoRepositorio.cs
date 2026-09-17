using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

public interface IExameCatalogoRepositorio
{
    Task<ExameCatalogo?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteNomeAsync(string nome, Guid? ignorarId = null, CancellationToken ct = default);
    Task<IReadOnlyList<ExameCatalogo>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default);
    Task AdicionarAsync(ExameCatalogo exame, CancellationToken ct = default);
    Task AtualizarAsync(ExameCatalogo exame, CancellationToken ct = default);
}

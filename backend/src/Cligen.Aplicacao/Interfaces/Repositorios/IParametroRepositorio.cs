using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

public interface IParametroRepositorio
{
    Task<Parametro?> ObterAsync(string chave, CancellationToken ct = default);
    Task AtualizarAsync(Parametro parametro, CancellationToken ct = default);
}

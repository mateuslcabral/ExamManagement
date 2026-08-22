using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

public interface IPacienteRepositorio
{
    /// <summary>Carrega o agregado completo (com responsável legal), excluído ou não.</summary>
    Task<Paciente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Verifica o índice único (tipo, número), ignorando o próprio paciente em edição. Inclui excluídos (P16).</summary>
    Task<bool> ExisteDocumentoAsync(TipoDocumento tipo, string numero, Guid? ignorarId = null, CancellationToken ct = default);

    /// <summary>Busca por nome, documento ou e-mail. Excluídos só entram se solicitado.</summary>
    Task<IReadOnlyList<Paciente>> ListarAsync(string? busca, bool incluirExcluidos, CancellationToken ct = default);

    Task AdicionarAsync(Paciente paciente, CancellationToken ct = default);
    Task AtualizarAsync(Paciente paciente, CancellationToken ct = default);
}

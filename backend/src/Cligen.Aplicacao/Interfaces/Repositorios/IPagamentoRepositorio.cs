using Cligen.Aplicacao.DTOs;
using Cligen.Dominio.Entidades;

namespace Cligen.Aplicacao.Interfaces.Repositorios;

/// <summary>Lançamentos excluídos logicamente e lançamentos de exames excluídos não aparecem em nenhuma consulta daqui.</summary>
public interface IPagamentoRepositorio
{
    Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Pagamento>> ListarPorExameAsync(Guid exameId, CancellationToken ct = default);
    Task AdicionarAsync(Pagamento pagamento, CancellationToken ct = default);
    Task AtualizarAsync(Pagamento pagamento, CancellationToken ct = default);

    /// <summary>Recebidos com data de recebimento no período (inclusive), mais antigos primeiro. Filtros opcionais.</summary>
    Task<IReadOnlyList<LinhaFinanceiraDto>> RecebidosNoPeriodoAsync(
        DateOnly de, DateOnly ate, Guid? convenioId, Guid? origemId, CancellationToken ct = default);

    /// <summary>Tudo que está a receber (convênio), sem corte de data, mais antigos primeiro. Filtros opcionais.</summary>
    Task<IReadOnlyList<LinhaFinanceiraDto>> AReceberAsync(Guid? convenioId, Guid? origemId, CancellationToken ct = default);
}

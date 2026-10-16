using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class PagamentoRepositorio(CligenDbContext db) : IPagamentoRepositorio
{
    public Task<Pagamento?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Pagamentos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Pagamento>> ListarPorExameAsync(Guid exameId, CancellationToken ct = default)
        => await db.Pagamentos.Where(p => p.ExameId == exameId).OrderBy(p => p.CriadoEm).ToListAsync(ct);

    public async Task AdicionarAsync(Pagamento pagamento, CancellationToken ct = default)
    {
        db.Pagamentos.Add(pagamento);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Pagamento pagamento, CancellationToken ct = default)
    {
        if (db.Entry(pagamento).State == EntityState.Detached)
            db.Pagamentos.Attach(pagamento).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }

    public Task<IReadOnlyList<LinhaFinanceiraDto>> RecebidosNoPeriodoAsync(DateOnly de, DateOnly ate, CancellationToken ct = default)
        => Linhas(p => p.Status == StatusPagamento.Recebido && p.DataRecebimento >= de && p.DataRecebimento <= ate, ct);

    public Task<IReadOnlyList<LinhaFinanceiraDto>> AReceberAsync(CancellationToken ct = default)
        => Linhas(p => p.Status == StatusPagamento.AReceber, ct);

    // A junção com Exame aplica o filtro de exclusão lógica do exame: pagamento de exame excluído some junto.
    private async Task<IReadOnlyList<LinhaFinanceiraDto>> Linhas(
        System.Linq.Expressions.Expression<Func<Pagamento, bool>> filtro, CancellationToken ct)
        => await (
            from p in db.Pagamentos.AsNoTracking().Where(filtro)
            join e in db.Exames on p.ExameId equals e.Id
            join pac in db.Pacientes on e.PacienteId equals pac.Id
            join c in db.ExamesCatalogo on e.ExameCatalogoId equals c.Id
            orderby p.DataRecebimento, p.CriadoEm
            select new LinhaFinanceiraDto(
                p.Id, e.Id, c.Nome, pac.Nome, pac.TipoDocumento, pac.NumeroDocumento,
                p.Metodo, p.Convenio, p.NumeroGuia, p.Valor, p.DataRecebimento, e.DataEntrada))
            .ToListAsync(ct);
}

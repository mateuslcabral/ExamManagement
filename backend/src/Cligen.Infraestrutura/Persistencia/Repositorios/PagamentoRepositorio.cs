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

    public Task<IReadOnlyList<LinhaFinanceiraDto>> RecebidosNoPeriodoAsync(
        DateOnly de, DateOnly ate, Guid? convenioId, Guid? origemId, CancellationToken ct = default)
        => Linhas(p => p.Status == StatusPagamento.Recebido && p.DataRecebimento >= de && p.DataRecebimento <= ate, convenioId, origemId, ct);

    public Task<IReadOnlyList<LinhaFinanceiraDto>> AReceberAsync(Guid? convenioId, Guid? origemId, CancellationToken ct = default)
        => Linhas(p => p.Status == StatusPagamento.AReceber, convenioId, origemId, ct);

    // A junção com Exame aplica o filtro de exclusão lógica do exame: pagamento de exame excluído some junto.
    private async Task<IReadOnlyList<LinhaFinanceiraDto>> Linhas(
        System.Linq.Expressions.Expression<Func<Pagamento, bool>> filtro, Guid? convenioId, Guid? origemId, CancellationToken ct)
    {
        var consulta =
            from p in db.Pagamentos.AsNoTracking().Where(filtro)
            join e in db.Exames on p.ExameId equals e.Id
            join pac in db.Pacientes on e.PacienteId equals pac.Id
            join c in db.ExamesCatalogo on e.ExameCatalogoId equals c.Id
            join o in db.Origens on e.OrigemId equals o.Id
            join cvj in db.Convenios on p.ConvenioId equals cvj.Id into cvg
            from cv in cvg.DefaultIfEmpty()
            select new { p, e, pac, c, o, cv };

        if (convenioId is { } idConvenio) consulta = consulta.Where(x => x.p.ConvenioId == idConvenio);
        if (origemId is { } idOrigem) consulta = consulta.Where(x => x.e.OrigemId == idOrigem);

        return await consulta
            .OrderBy(x => x.p.DataRecebimento).ThenBy(x => x.p.CriadoEm)
            .Select(x => new LinhaFinanceiraDto(
                x.p.Id, x.e.Id, "EX-" + x.e.Numero.ToString("D6"), x.c.Nome,
                x.pac.Nome, x.pac.TipoDocumento, x.pac.NumeroDocumento,
                x.o.Id, x.o.Nome,
                x.p.Metodo, x.p.ConvenioId, x.cv != null ? x.cv.Nome : null, x.p.NumeroGuia,
                x.p.Valor, x.p.DataRecebimento, x.e.DataEntrada))
            .ToListAsync(ct);
    }
}

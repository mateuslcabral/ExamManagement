using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class ExameCatalogoRepositorio(CligenDbContext db) : IExameCatalogoRepositorio
{
    public Task<ExameCatalogo?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.ExamesCatalogo.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExisteNomeAsync(string nome, Guid? ignorarId = null, CancellationToken ct = default)
        => db.ExamesCatalogo.AnyAsync(e => e.Nome == nome && (ignorarId == null || e.Id != ignorarId), ct);

    public async Task<IReadOnlyList<ExameCatalogo>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default)
        => await db.ExamesCatalogo.AsNoTracking()
            .Where(e => !apenasAtivos || e.Ativo)
            .OrderByDescending(e => e.Ativo).ThenBy(e => e.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(ExameCatalogo exame, CancellationToken ct = default)
    {
        db.ExamesCatalogo.Add(exame);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(ExameCatalogo exame, CancellationToken ct = default)
    {
        // Mesmo cuidado de UsuarioRepositorio: não usar Update(), que marcaria a identity Seq como modificada.
        if (db.Entry(exame).State == EntityState.Detached)
            db.ExamesCatalogo.Attach(exame).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

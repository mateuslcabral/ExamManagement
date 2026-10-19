using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class CadastroRepositorio<T>(CligenDbContext db) : ICadastroRepositorio<T> where T : CadastroSimples
{
    private DbSet<T> Set => db.Set<T>();

    public Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExisteNomeAsync(string nome, Guid? ignorarId = null, CancellationToken ct = default)
        => Set.AnyAsync(c => c.Nome == nome && (ignorarId == null || c.Id != ignorarId), ct);

    public async Task<IReadOnlyList<T>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Where(c => !apenasAtivos || c.Ativo)
            .OrderByDescending(c => c.Ativo).ThenBy(c => c.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(T item, CancellationToken ct = default)
    {
        Set.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(T item, CancellationToken ct = default)
    {
        if (db.Entry(item).State == EntityState.Detached)
            Set.Attach(item).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

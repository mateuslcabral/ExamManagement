using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class UsuarioRepositorio(CligenDbContext db) : IUsuarioRepositorio
{
    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default)
        => db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default)
        => db.Usuarios.AnyAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default)
        => await db.Usuarios.AsNoTracking().OrderBy(u => u.Nome).ToListAsync(ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
    {
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Usuario usuario, CancellationToken ct = default)
    {
        // A entidade já vem rastreada (ObterPorId/ObterPorEmail); o change tracker detecta só o que mudou.
        // Não usar Update(): marcaria todas as colunas como modificadas, inclusive a identity Seq.
        if (db.Entry(usuario).State == EntityState.Detached)
            db.Usuarios.Attach(usuario).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

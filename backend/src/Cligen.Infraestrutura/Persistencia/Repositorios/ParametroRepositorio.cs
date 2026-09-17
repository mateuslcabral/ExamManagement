using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class ParametroRepositorio(CligenDbContext db) : IParametroRepositorio
{
    public Task<Parametro?> ObterAsync(string chave, CancellationToken ct = default)
        => db.Parametros.FirstOrDefaultAsync(p => p.Chave == chave, ct);

    public async Task AtualizarAsync(Parametro parametro, CancellationToken ct = default)
    {
        if (db.Entry(parametro).State == EntityState.Detached)
            db.Parametros.Attach(parametro).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

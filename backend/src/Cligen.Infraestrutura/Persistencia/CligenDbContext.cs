using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia;

public sealed class CligenDbContext(DbContextOptions<CligenDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API (não Data Annotations) para manter Cligen.Dominio livre de dependências do EF Core.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CligenDbContext).Assembly);
    }
}

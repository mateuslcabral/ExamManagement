using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia;

public sealed class CligenDbContext(DbContextOptions<CligenDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ExameCatalogo> ExamesCatalogo => Set<ExameCatalogo>();
    public DbSet<Parametro> Parametros => Set<Parametro>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Exame> Exames => Set<Exame>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        // Todo DateTime do domínio é UTC. O SQL Server não guarda o Kind: sem isto, o valor lido volta como
        // Unspecified e é serializado sem o "Z", e o navegador o interpreta como hora local (erro de 3 h).
        builder.Properties<DateTime>().HaveConversion<ConversorDateTimeUtc>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API (não Data Annotations) para manter Cligen.Dominio livre de dependências do EF Core.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CligenDbContext).Assembly);
    }
}

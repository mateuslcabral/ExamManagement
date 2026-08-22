using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class PacienteConfiguracao : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> b)
    {
        b.ToTable("Paciente");

        // C1: PK GUID não agrupada; Seq sequencial interno para ordenação física e futuro número legível (D2.1 / P17).
        b.HasKey(p => p.Id).IsClustered(false);
        b.Property(p => p.Id).ValueGeneratedNever(); // GUID gerado no domínio: novo registro via navegação deve ser Added, não Modified
        b.Property<long>("Seq").ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        b.Property(p => p.Nome).HasMaxLength(150).IsRequired();
        b.Property(p => p.DataNascimento).HasColumnType("date").IsRequired();

        b.Property(p => p.TipoDocumento).HasConversion<int>().IsRequired();
        b.Property(p => p.NumeroDocumento).HasMaxLength(20).IsRequired();
        // Índice único inclusive sobre excluídos (P16): recadastrar o mesmo documento continua bloqueado.
        b.HasIndex(p => new { p.TipoDocumento, p.NumeroDocumento }).IsUnique();

        b.Property(p => p.Email).HasMaxLength(254).IsRequired();
        b.Property(p => p.Telefone).HasMaxLength(13).IsRequired();

        b.Property(p => p.ExcluidoEm);
        b.Property(p => p.ExcluidoPorUsuarioId);
        b.Property(p => p.MotivoExclusao).HasMaxLength(500);
        b.Ignore(p => p.Excluido);

        b.Property(p => p.CriadoEm).IsRequired();
        b.Property(p => p.AtualizadoEm);

        b.HasOne(p => p.ResponsavelLegal)
            .WithOne()
            .HasForeignKey<ResponsavelLegal>(r => r.PacienteId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(p => p.ResponsavelLegal).AutoInclude();
    }
}

public sealed class ResponsavelLegalConfiguracao : IEntityTypeConfiguration<ResponsavelLegal>
{
    public void Configure(EntityTypeBuilder<ResponsavelLegal> b)
    {
        b.ToTable("ResponsavelLegal");
        b.HasKey(r => r.Id).IsClustered(false);
        b.Property(r => r.Id).ValueGeneratedNever(); // GUID gerado no domínio: novo registro via navegação deve ser Added, não Modified
        b.HasIndex(r => r.PacienteId).IsUnique().IsClustered();

        b.Property(r => r.Nome).HasMaxLength(150).IsRequired();
        b.Property(r => r.TipoDocumento).HasConversion<int>().IsRequired();
        b.Property(r => r.NumeroDocumento).HasMaxLength(20).IsRequired();
        b.Property(r => r.Parentesco).HasMaxLength(50);
    }
}

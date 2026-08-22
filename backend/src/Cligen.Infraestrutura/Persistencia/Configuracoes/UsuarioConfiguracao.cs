using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuario");

        // PK GUID não agrupada + chave sequencial interna para ordenação física,
        // mesma decisão registrada para Paciente (C1 / docs/01-stack).
        b.HasKey(u => u.Id).IsClustered(false);
        b.Property<long>("Seq").ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        b.Property(u => u.Nome).HasMaxLength(150).IsRequired();
        b.Property(u => u.Email).HasMaxLength(254).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();

        b.Property(u => u.TipoLogin).HasConversion<int>().IsRequired();
        b.Property(u => u.SenhaHash).HasMaxLength(500);
        b.Property(u => u.GoogleSubjectId).HasMaxLength(255);
        b.HasIndex(u => u.GoogleSubjectId).IsUnique().HasFilter("[GoogleSubjectId] IS NOT NULL");

        b.Property(u => u.Ativo).IsRequired();
        b.Property(u => u.CriadoEm).IsRequired();
        b.Property(u => u.AtualizadoEm);
    }
}

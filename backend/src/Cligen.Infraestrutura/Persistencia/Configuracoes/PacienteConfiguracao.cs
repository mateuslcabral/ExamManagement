using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class PacienteConfiguracao : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> b)
    {
        b.ToTable("Paciente");

        // PK GUID não agrupada + chave sequencial interna para ordenação física (C1).
        b.HasKey(p => p.Id).IsClustered(false);
        b.Property<long>("Seq").ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        b.Property(p => p.Nome).HasMaxLength(Paciente.TamanhoMaximoNome).IsRequired();
        b.HasIndex(p => p.Nome);
        b.Property(p => p.DataNascimento).IsRequired();

        b.Property(p => p.TipoDocumento).HasConversion<int>().IsRequired();
        b.Property(p => p.NumeroDocumento).HasMaxLength(20).IsRequired();
        b.HasIndex(p => new { p.TipoDocumento, p.NumeroDocumento }).IsUnique(); // "Esse CPF já foi utilizado" (Q11)
        b.Ignore(p => p.Documento);

        b.Property(p => p.Email).HasMaxLength(254).IsRequired();
        b.Property(p => p.Telefone).HasMaxLength(16).IsRequired();

        b.OwnsOne(p => p.ResponsavelLegal, r =>
        {
            r.ToTable("ResponsavelLegal");
            r.WithOwner().HasForeignKey("PacienteId");
            r.HasKey("PacienteId");
            r.Property(x => x.Nome).HasMaxLength(ResponsavelLegal.TamanhoMaximoNome).IsRequired();
            r.Property(x => x.TipoDocumento).HasConversion<int>().IsRequired();
            r.Property(x => x.NumeroDocumento).HasMaxLength(20).IsRequired();
            r.Property(x => x.Parentesco).HasMaxLength(ResponsavelLegal.TamanhoMaximoParentesco);
            r.Ignore(x => x.Documento);
        });

        // Autoria: usuário nunca é apagado (só desativado), então Restrict não bloqueia a operação.
        b.Property(p => p.CriadoEm).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.Property(p => p.AtualizadoEm);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.AtualizadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}

using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class ExameCatalogoConfiguracao : IEntityTypeConfiguration<ExameCatalogo>
{
    public void Configure(EntityTypeBuilder<ExameCatalogo> b)
    {
        b.ToTable("ExameCatalogo");

        // PK GUID não agrupada + chave sequencial interna para ordenação física (mesmo padrão de Usuario).
        b.HasKey(e => e.Id).IsClustered(false);
        b.Property<long>("Seq").ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        // Collation padrão do SQL Server é case-insensitive: "Exoma" e "exoma" colidem no índice.
        b.Property(e => e.Nome).HasMaxLength(ExameCatalogo.TamanhoMaximoNome).IsRequired();
        b.HasIndex(e => e.Nome).IsUnique();

        b.Property(e => e.PrazoDias).IsRequired();
        b.Property(e => e.PrecoReferencia).HasPrecision(18, 2).IsRequired();
        b.Property(e => e.PrecoLimite).HasPrecision(18, 2).IsRequired();
        b.Property(e => e.NumeroGenes);
        b.Property(e => e.Genes).HasMaxLength(ExameCatalogo.TamanhoMaximoTexto);
        b.Property(e => e.InformacoesTecnicas).HasMaxLength(ExameCatalogo.TamanhoMaximoTexto);
        b.HasOne<Destino>().WithMany().HasForeignKey(e => e.DestinoId).OnDelete(DeleteBehavior.Restrict);

        b.Property(e => e.Ativo).IsRequired();
        b.Property(e => e.CriadoEm).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.Property(e => e.AtualizadoEm);
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.AtualizadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}

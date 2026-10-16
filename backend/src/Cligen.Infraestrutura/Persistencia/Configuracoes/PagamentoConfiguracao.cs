using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class PagamentoConfiguracao : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> b)
    {
        b.ToTable("Pagamento");

        b.HasKey(p => p.Id).IsClustered(false);
        b.Property<long>("Seq").ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        // Exclusão lógica (P4). Para auditoria, IgnoreQueryFilters().
        b.HasQueryFilter(p => p.ExcluidoEm == null);

        b.HasOne<Exame>().WithMany().HasForeignKey(p => p.ExameId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(p => p.ExameId);

        b.Property(p => p.Metodo).HasConversion<int>().IsRequired();
        b.Property(p => p.Valor).HasPrecision(18, 2).IsRequired();
        b.Property(p => p.Status).HasConversion<int>().IsRequired();
        b.HasIndex(p => new { p.Status, p.DataRecebimento }); // relatório por regime de caixa
        b.Property(p => p.DataRecebimento);
        b.Property(p => p.Convenio).HasMaxLength(Pagamento.TamanhoMaximoConvenio);
        b.Property(p => p.NumeroGuia).HasMaxLength(Pagamento.TamanhoMaximoGuia);
        b.Property(p => p.Parcelas);
        b.Property(p => p.ValorParcela).HasPrecision(18, 2);
        b.Property(p => p.Observacao).HasMaxLength(Pagamento.TamanhoMaximoObservacao);
        b.Property(p => p.MotivoGlosa).HasMaxLength(Pagamento.TamanhoMaximoMotivo);
        b.Ignore(p => p.Excluido);
        b.Ignore(p => p.ContaComoRecebido);
        b.Ignore(p => p.ContaComoAReceber);

        b.Property(p => p.CriadoEm).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.AtualizadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.ExcluidoPorId).OnDelete(DeleteBehavior.Restrict);
        b.Property(p => p.MotivoExclusao).HasMaxLength(Pagamento.TamanhoMaximoMotivo);
    }
}

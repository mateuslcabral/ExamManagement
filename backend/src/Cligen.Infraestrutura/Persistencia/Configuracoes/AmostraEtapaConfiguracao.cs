using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class AmostraConfiguracao : IEntityTypeConfiguration<Amostra>
{
    public void Configure(EntityTypeBuilder<Amostra> b)
    {
        b.ToTable("Amostra");
        b.HasKey(a => a.Id).IsClustered(false);
        b.Property(a => a.Id).ValueGeneratedNever();
        b.HasIndex(a => a.ExameId).IsClustered();

        b.Property(a => a.DataAcolhimento).HasColumnType("date").IsRequired();
        b.Property(a => a.Situacao).HasConversion<int>().IsRequired();
        b.Property(a => a.RegistradoPorUsuarioId).IsRequired();
        b.Property(a => a.CriadoEm).IsRequired();
        b.Property(a => a.MotivoRejeicao).HasMaxLength(500);
        b.Property(a => a.RejeitadaEm);
        b.Property(a => a.RejeitadaPorUsuarioId);
        b.Ignore(a => a.Ativa);
    }
}

public sealed class EtapaAndamentoConfiguracao : IEntityTypeConfiguration<EtapaAndamento>
{
    public void Configure(EntityTypeBuilder<EtapaAndamento> b)
    {
        b.ToTable("EtapaAndamento");
        b.HasKey(e => e.Id).IsClustered(false);
        b.Property(e => e.Id).ValueGeneratedNever();
        // Uma etapa de cada tipo por exame (Q21: as três sempre presentes, nunca duplicadas).
        b.HasIndex(e => new { e.ExameId, e.Tipo }).IsUnique().IsClustered();

        b.Property(e => e.Tipo).HasConversion<int>().IsRequired();
        b.Property(e => e.Data).IsRequired();
        b.Property(e => e.NomeOriginal).HasMaxLength(255).IsRequired();
        b.Property(e => e.Caminho).HasMaxLength(500).IsRequired();
        b.Property(e => e.TamanhoBytes).IsRequired();
        b.Property(e => e.HashSha256).HasMaxLength(64).IsRequired();
        b.Property(e => e.RegistradoPorUsuarioId).IsRequired();
        b.Property(e => e.ArquivoSubstituidoEm);
        b.Property(e => e.Substituicoes).IsRequired();
    }
}

using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class ExameConfiguracao : IEntityTypeConfiguration<Exame>
{
    public void Configure(EntityTypeBuilder<Exame> b)
    {
        b.ToTable("Exame");

        b.HasKey(e => e.Id).IsClustered(false);
        b.Property<long>("Seq").ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        // Exclusão lógica: excluído sai de todas as consultas (C2). Para auditoria, use IgnoreQueryFilters().
        b.HasQueryFilter(e => e.ExcluidoEm == null);

        b.HasOne<Paciente>().WithMany().HasForeignKey(e => e.PacienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ExameCatalogo>().WithMany().HasForeignKey(e => e.ExameCatalogoId).OnDelete(DeleteBehavior.Restrict);

        b.Property(e => e.Origem).HasConversion<int>().IsRequired();
        b.Property(e => e.Destino).HasMaxLength(Exame.TamanhoMaximoDestino);
        b.Property(e => e.TipoMedico).HasConversion<int>().IsRequired();
        b.Property(e => e.NomeMedicoExterno).HasMaxLength(Exame.TamanhoMaximoNomeMedico);
        b.Property(e => e.Preco).HasPrecision(18, 2).IsRequired();
        b.Property(e => e.DataEntrada).IsRequired();
        b.Property(e => e.Estado).HasConversion<int>().IsRequired();
        b.HasIndex(e => e.Estado);
        b.Property(e => e.DataLiberacaoPrevista);
        b.Property(e => e.DataLiberacaoEfetiva);
        b.Ignore(e => e.AmostraVigente);
        b.Ignore(e => e.Excluido);
        b.Ignore(e => e.NomeMedico);

        b.OwnsMany(e => e.Anexos, a =>
        {
            a.ToTable("Anexo");
            a.WithOwner().HasForeignKey("ExameId");
            a.HasKey(x => x.Id);
            // Id gerado no domínio: sem isto o EF trataria um anexo novo com Id preenchido como já existente.
            a.Property(x => x.Id).ValueGeneratedNever();
            a.Property(x => x.NomeOriginal).HasMaxLength(Anexo.TamanhoMaximoNomeOriginal).IsRequired();
            a.Property(x => x.TipoConteudo).HasMaxLength(100).IsRequired();
            a.Property(x => x.TamanhoBytes).IsRequired();
            a.Property(x => x.HashSha256).HasMaxLength(64).IsFixedLength().IsUnicode(false).IsRequired();
            a.Property(x => x.Caminho).HasMaxLength(500).IsRequired();
            a.Property(x => x.EnviadoEm).IsRequired();
            a.HasOne<Usuario>().WithMany().HasForeignKey(x => x.EnviadoPorId).OnDelete(DeleteBehavior.Restrict);
            a.HasOne<Usuario>().WithMany().HasForeignKey(x => x.RemovidoPorId).OnDelete(DeleteBehavior.Restrict);
            a.Ignore(x => x.Ativo);
        });
        b.Navigation(e => e.Anexos).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.OwnsMany(e => e.Amostras, a =>
        {
            a.ToTable("Amostra");
            a.WithOwner().HasForeignKey("ExameId");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).ValueGeneratedNever(); // Id gerado no domínio (mesmo motivo de Anexo)
            a.Property(x => x.DataAcolhimento).IsRequired();
            a.Property(x => x.PrazoExecucaoDias).IsRequired();
            a.Property(x => x.DiasRevisao).IsRequired();
            a.Property(x => x.DataLiberacaoPrevista).IsRequired();
            a.Property(x => x.Recoleta).IsRequired();
            a.Property(x => x.RegistradoEm).IsRequired();
            a.HasOne<Usuario>().WithMany().HasForeignKey(x => x.RegistradoPorId).OnDelete(DeleteBehavior.Restrict);
            a.HasOne<Usuario>().WithMany().HasForeignKey(x => x.RejeitadaPorId).OnDelete(DeleteBehavior.Restrict);
            a.Property(x => x.MotivoRejeicao).HasMaxLength(Amostra.TamanhoMaximoMotivoRejeicao);
            a.Ignore(x => x.Rejeitada);
        });
        b.Navigation(e => e.Amostras).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.OwnsMany(e => e.Etapas, a =>
        {
            a.ToTable("EtapaAndamento");
            a.WithOwner().HasForeignKey("ExameId");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).ValueGeneratedNever();
            a.Property(x => x.Tipo).HasConversion<int>().IsRequired();
            a.HasIndex("ExameId", nameof(EtapaAndamento.Tipo)).IsUnique(); // uma etapa de cada tipo por exame
            a.Property(x => x.Data).IsRequired();
            a.Property(x => x.NomeOriginal).HasMaxLength(Anexo.TamanhoMaximoNomeOriginal).IsRequired();
            a.Property(x => x.Caminho).HasMaxLength(500).IsRequired();
            a.Property(x => x.TamanhoBytes).IsRequired();
            a.Property(x => x.HashSha256).HasMaxLength(64).IsFixedLength().IsUnicode(false).IsRequired();
            a.HasOne<Usuario>().WithMany().HasForeignKey(x => x.RegistradoPorId).OnDelete(DeleteBehavior.Restrict);
            a.Property(x => x.Substituicoes).IsRequired();
            a.HasOne<Usuario>().WithMany().HasForeignKey(x => x.SubstituidoPorId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Navigation(e => e.Etapas).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.Property(e => e.CriadoEm).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.AtualizadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.ExcluidoPorId).OnDelete(DeleteBehavior.Restrict);
        b.Property(e => e.MotivoExclusao).HasMaxLength(Exame.TamanhoMaximoMotivoExclusao);
    }
}

using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class ParametroConfiguracao : IEntityTypeConfiguration<Parametro>
{
    public void Configure(EntityTypeBuilder<Parametro> b)
    {
        b.ToTable("Parametro");
        b.HasKey(p => p.Chave);
        b.Property(p => p.Chave).HasMaxLength(50);
        b.Property(p => p.Valor).HasMaxLength(200).IsRequired();
        b.Property(p => p.Descricao).HasMaxLength(300).IsRequired();
        b.Property(p => p.AtualizadoEm);

        // P13/P28: os 3 dias de revisão como parâmetro global editável.
        b.HasData(new { Chave = Parametro.ChaveDiasRevisao, Valor = "3", Descricao = "Dias de revisão da Cligen somados ao prazo de execução de todo exame (dias corridos)." });
    }
}

public sealed class ExameCatalogoConfiguracao : IEntityTypeConfiguration<ExameCatalogo>
{
    public void Configure(EntityTypeBuilder<ExameCatalogo> b)
    {
        b.ToTable("ExameCatalogo");
        b.HasKey(e => e.Id).IsClustered(false);
        b.Property(e => e.Id).ValueGeneratedNever(); // GUID gerado no domínio: novo registro via navegação deve ser Added, não Modified
        b.Property<long>("Seq").ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        b.Property(e => e.Nome).HasMaxLength(200).IsRequired();
        b.HasIndex(e => e.Nome).IsUnique();
        b.Property(e => e.PrazoExecucaoDias).IsRequired();
        b.Property(e => e.PrecoReferencia).HasPrecision(12, 2).IsRequired();
        b.Property(e => e.Ativo).IsRequired();
        b.Property(e => e.CriadoEm).IsRequired();
        b.Property(e => e.AtualizadoEm);
    }
}

public sealed class ExameConfiguracao : IEntityTypeConfiguration<Exame>
{
    public void Configure(EntityTypeBuilder<Exame> b)
    {
        b.ToTable("Exame");
        b.HasKey(e => e.Id).IsClustered(false);
        b.Property(e => e.Id).ValueGeneratedNever(); // GUID gerado no domínio: novo registro via navegação deve ser Added, não Modified
        b.Property<long>("Seq").ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        b.HasOne(e => e.Paciente).WithMany().HasForeignKey(e => e.PacienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.ExameCatalogo).WithMany().HasForeignKey(e => e.ExameCatalogoId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => e.PacienteId);

        b.Property(e => e.Origem).HasConversion<int>().IsRequired();
        b.Property(e => e.Destino).HasMaxLength(200);
        b.Property(e => e.TipoMedico).HasConversion<int>().IsRequired();
        b.Property(e => e.NomeMedico).HasMaxLength(150).IsRequired();
        b.Property(e => e.DataEntrada).HasColumnType("date").IsRequired();
        b.Property(e => e.Preco).HasPrecision(12, 2).IsRequired();
        b.Property(e => e.Estado).HasConversion<int>().IsRequired();
        b.Property(e => e.DataLiberacaoPrevista).HasColumnType("date");
        b.Property(e => e.DataLiberacaoEfetiva);

        b.Property(e => e.ExcluidoEm);
        b.Property(e => e.ExcluidoPorUsuarioId);
        b.Property(e => e.MotivoExclusao).HasMaxLength(500);
        b.Ignore(e => e.Excluido);

        b.Property(e => e.CriadoEm).IsRequired();
        b.Property(e => e.AtualizadoEm);

        // Anexos: coleção privada (_anexos) exposta como IReadOnlyCollection.
        b.HasMany(e => e.Anexos).WithOne().HasForeignKey(a => a.ExameId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(e => e.Anexos).HasField("_anexos").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        b.HasMany(e => e.Amostras).WithOne().HasForeignKey(a => a.ExameId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(e => e.Amostras).HasField("_amostras").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        b.HasMany(e => e.Etapas).WithOne().HasForeignKey(a => a.ExameId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(e => e.Etapas).HasField("_etapas").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        b.Ignore(e => e.AmostraAtiva);
        b.Ignore(e => e.Disponibilizado);
        b.Navigation(e => e.Paciente).AutoInclude();
        b.Navigation(e => e.ExameCatalogo).AutoInclude();
    }
}

public sealed class AnexoConfiguracao : IEntityTypeConfiguration<Anexo>
{
    public void Configure(EntityTypeBuilder<Anexo> b)
    {
        b.ToTable("Anexo");
        b.HasKey(a => a.Id).IsClustered(false);
        b.Property(a => a.Id).ValueGeneratedNever(); // GUID gerado no domínio: novo registro via navegação deve ser Added, não Modified
        b.HasIndex(a => a.ExameId).IsClustered();

        b.Property(a => a.NomeOriginal).HasMaxLength(255).IsRequired();
        b.Property(a => a.Caminho).HasMaxLength(500).IsRequired();
        b.Property(a => a.TipoConteudo).HasMaxLength(100).IsRequired();
        b.Property(a => a.TamanhoBytes).IsRequired();
        b.Property(a => a.HashSha256).HasMaxLength(64).IsRequired();
        b.Property(a => a.EnviadoEm).IsRequired();
    }
}

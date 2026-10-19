using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento comum de Origem, Destino e Convênio: uma tabela por tipo, nome único (collation CI).</summary>
public abstract class CadastroSimplesConfiguracao<T>(string tabela) : IEntityTypeConfiguration<T> where T : CadastroSimples
{
    public void Configure(EntityTypeBuilder<T> b)
    {
        b.ToTable(tabela);

        b.HasKey(c => c.Id).IsClustered(false);
        b.Property<long>("Seq").ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        b.HasIndex("Seq").IsUnique().IsClustered();

        b.Property(c => c.Nome).HasMaxLength(CadastroSimples.TamanhoMaximoNome).IsRequired();
        b.HasIndex(c => c.Nome).IsUnique();
        b.Property(c => c.Ativo).IsRequired();

        b.Property(c => c.CriadoEm).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.CriadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.AtualizadoPorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OrigemConfiguracao() : CadastroSimplesConfiguracao<Origem>("Origem");
public sealed class DestinoConfiguracao() : CadastroSimplesConfiguracao<Destino>("Destino");
public sealed class ConvenioConfiguracao() : CadastroSimplesConfiguracao<Convenio>("Convenio");

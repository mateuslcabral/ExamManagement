using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cligen.Infraestrutura.Persistencia.Configuracoes;

public sealed class ParametroConfiguracao : IEntityTypeConfiguration<Parametro>
{
    public void Configure(EntityTypeBuilder<Parametro> b)
    {
        b.ToTable("Parametro");

        b.HasKey(p => p.Chave);
        b.Property(p => p.Chave).HasMaxLength(100);
        b.Property(p => p.Valor).HasMaxLength(500).IsRequired();
        b.Property(p => p.AtualizadoEm);

        // Valores iniciais entram pela migration: o sistema não funciona sem eles.
        var diasRevisao = Parametro.CriarDiasRevisao();
        b.HasData(new { diasRevisao.Chave, diasRevisao.Valor, AtualizadoEm = (DateTime?)null });
    }
}

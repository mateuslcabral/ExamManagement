using Cligen.Aplicacao.Interfaces.Servicos;

namespace Cligen.Infraestrutura.Tempo;

/// <summary>Data de hoje no fuso de Brasília (sede da Cligen). Cálculo de idade/maioridade depende disso.</summary>
public sealed class RelogioSistema : IRelogio
{
    private static readonly TimeZoneInfo Fuso = ObterFuso();

    public DateOnly Hoje => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Fuso));

    private static TimeZoneInfo ObterFuso()
    {
        foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
        }
        return TimeZoneInfo.Utc;
    }
}

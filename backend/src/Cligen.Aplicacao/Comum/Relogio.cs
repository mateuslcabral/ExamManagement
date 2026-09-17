namespace Cligen.Aplicacao.Comum;

/// <summary>Data civil da clínica. Regras de calendário (menoridade, prazos) usam o fuso de Brasília, não UTC.</summary>
public static class Relogio
{
    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateOnly Hoje() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Brasilia));
}

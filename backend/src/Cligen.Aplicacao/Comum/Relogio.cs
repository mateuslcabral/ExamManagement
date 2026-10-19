namespace Cligen.Aplicacao.Comum;

/// <summary>Data civil da clínica. Regras de calendário (menoridade, prazos) usam o fuso de Brasília, não UTC.</summary>
public static class Relogio
{
    private static readonly TimeZoneInfo Brasilia = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateOnly Hoje() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Brasilia));

    /// <summary>Instante UTC em que o dia civil de Brasília começa — para comparar DateTime UTC com datas de calendário.</summary>
    public static DateTime InicioDoDiaUtc(DateOnly dia)
        => TimeZoneInfo.ConvertTimeToUtc(dia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), Brasilia);
}

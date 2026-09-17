using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cligen.Infraestrutura.Persistencia;

/// <summary>Marca como UTC todo DateTime lido do banco (o SQL Server não preserva o Kind).</summary>
public sealed class ConversorDateTimeUtc() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

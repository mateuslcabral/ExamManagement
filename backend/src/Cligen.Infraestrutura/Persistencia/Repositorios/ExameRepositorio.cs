using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class ExameRepositorio(CligenDbContext db) : IExameRepositorio
{
    public Task<Exame?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Exames.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<Exame?> ObterExcluidoPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Exames.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id && e.ExcluidoEm != null, ct);

    public async Task<(IReadOnlyList<ExameResumoDto> Itens, int Total)> BuscarAsync(
        string? busca, Guid? pacienteId, EstadoExame? estado, bool excluidos, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var exames = excluidos
            ? db.Exames.IgnoreQueryFilters().Where(e => e.ExcluidoEm != null)
            : db.Exames;

        // Paciente e catálogo são agregados separados (sem navegação no domínio): junção explícita.
        var consulta =
            from e in exames.AsNoTracking()
            join p in db.Pacientes on e.PacienteId equals p.Id
            join c in db.ExamesCatalogo on e.ExameCatalogoId equals c.Id
            select new { e, p, c };

        if (pacienteId is { } idPaciente)
            consulta = consulta.Where(x => x.e.PacienteId == idPaciente);
        if (estado is { } filtroEstado)
            consulta = consulta.Where(x => x.e.Estado == filtroEstado);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var documento = new string(busca.Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant();
            var buscarDocumento = documento.Length >= 3;
            consulta = consulta.Where(x =>
                x.p.Nome.Contains(busca) || x.c.Nome.Contains(busca)
                || (buscarDocumento && x.p.NumeroDocumento.StartsWith(documento)));
        }

        var total = await consulta.CountAsync(ct);
        var itens = await consulta
            .OrderByDescending(x => x.e.DataEntrada).ThenByDescending(x => EF.Property<long>(x.e, "Seq"))
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new ExameResumoDto(
                x.e.Id, x.p.Id, x.p.Nome, x.p.TipoDocumento, x.p.NumeroDocumento, x.c.Id, x.c.Nome, x.e.Origem, x.e.DataEntrada, x.e.Estado, x.e.DataLiberacaoPrevista,
                x.e.ExcluidoEm, x.e.MotivoExclusao))
            .ToListAsync(ct);
        return (itens, total);
    }

    public async Task AdicionarAsync(Exame exame, CancellationToken ct = default)
    {
        db.Exames.Add(exame);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Exame exame, CancellationToken ct = default)
    {
        // Entidade rastreada desde ObterPorIdAsync; anexos novos são detectados como inseridos pelo change tracker.
        if (db.Entry(exame).State == EntityState.Detached)
            db.Exames.Attach(exame).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

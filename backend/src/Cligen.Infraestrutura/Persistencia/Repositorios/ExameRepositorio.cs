using Cligen.Aplicacao.Comum;
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
        string? busca, Guid? pacienteId, EstadoExame? estado, bool excluidos,
        CampoDataExame? campoData, DateOnly? de, DateOnly? ate,
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var exames = excluidos
            ? db.Exames.IgnoreQueryFilters().Where(e => e.ExcluidoEm != null)
            : db.Exames;

        // Paciente, catálogo, origem e destino são agregados separados (sem navegação no domínio): junções explícitas.
        var consulta =
            from e in exames.AsNoTracking()
            join p in db.Pacientes on e.PacienteId equals p.Id
            join c in db.ExamesCatalogo on e.ExameCatalogoId equals c.Id
            join o in db.Origens on e.OrigemId equals o.Id
            join dj in db.Destinos on e.DestinoId equals dj.Id into dg
            from d in dg.DefaultIfEmpty()
            select new { e, p, c, o, d };

        if (pacienteId is { } idPaciente)
            consulta = consulta.Where(x => x.e.PacienteId == idPaciente);
        if (estado is { } filtroEstado)
            consulta = consulta.Where(x => x.e.Estado == filtroEstado);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var documento = new string(busca.Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant();
            var buscarDocumento = documento.Length >= 3;
            // "EX-000123" ou "123" também encontra pelo código interno.
            var numero = long.TryParse(busca.Trim().Replace("EX-", "", StringComparison.OrdinalIgnoreCase), out var n) ? n : -1;
            consulta = consulta.Where(x =>
                x.p.Nome.Contains(busca) || x.c.Nome.Contains(busca) || x.e.Numero == numero
                || (buscarDocumento && x.p.NumeroDocumento.StartsWith(documento)));
        }

        if (campoData is { } campo && (de is not null || ate is not null))
        {
            switch (campo)
            {
                case CampoDataExame.Acolhimento:
                    consulta = consulta.Where(x => x.e.DataAcolhimento != null
                        && (de == null || x.e.DataAcolhimento >= de) && (ate == null || x.e.DataAcolhimento <= ate));
                    break;
                case CampoDataExame.Prevista:
                    consulta = consulta.Where(x => x.e.DataLiberacaoPrevista != null
                        && (de == null || x.e.DataLiberacaoPrevista >= de) && (ate == null || x.e.DataLiberacaoPrevista <= ate));
                    break;
                case CampoDataExame.Disponibilizado:
                    // Instante UTC; o "dia" é o de Brasília.
                    var inicio = de is { } d0 ? Relogio.InicioDoDiaUtc(d0) : (DateTime?)null;
                    var fim = ate is { } d1 ? Relogio.InicioDoDiaUtc(d1.AddDays(1)) : (DateTime?)null;
                    consulta = consulta.Where(x => x.e.DataLiberacaoEfetiva != null
                        && (inicio == null || x.e.DataLiberacaoEfetiva >= inicio) && (fim == null || x.e.DataLiberacaoEfetiva < fim));
                    break;
            }
        }

        var ordenada = campoData switch
        {
            CampoDataExame.Acolhimento => consulta.OrderBy(x => x.e.DataAcolhimento).ThenBy(x => x.e.Numero),
            CampoDataExame.Prevista => consulta.OrderBy(x => x.e.DataLiberacaoPrevista).ThenBy(x => x.e.Numero),
            CampoDataExame.Disponibilizado => consulta.OrderBy(x => x.e.DataLiberacaoEfetiva).ThenBy(x => x.e.Numero),
            _ => consulta.OrderByDescending(x => x.e.DataEntrada).ThenByDescending(x => x.e.Numero)
        };

        var total = await consulta.CountAsync(ct);
        var itens = await ordenada
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new ExameResumoDto(
                x.e.Id, "EX-" + x.e.Numero.ToString("D6"),
                x.p.Id, x.p.Nome, x.p.TipoDocumento, x.p.NumeroDocumento,
                x.c.Id, x.c.Nome, x.o.Id, x.o.Nome, x.e.DestinoId, x.d != null ? x.d.Nome : null,
                x.e.DataEntrada, x.e.Estado, x.e.DataAcolhimento, x.e.DataLiberacaoPrevista, x.e.DataLiberacaoEfetiva,
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
        // Entidade rastreada desde ObterPorIdAsync; anexos/amostras/comentários novos são detectados como inseridos.
        if (db.Entry(exame).State == EntityState.Detached)
            db.Exames.Attach(exame).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

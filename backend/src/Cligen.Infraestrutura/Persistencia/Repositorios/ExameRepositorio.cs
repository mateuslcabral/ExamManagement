using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class ExameRepositorio(CligenDbContext db) : IExameRepositorio
{
    // Paciente, ExameCatalogo e Anexos vêm por AutoInclude (configuração).
    public Task<Exame?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Exames.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Exame>> ListarAsync(string? busca, bool incluirExcluidos, CancellationToken ct = default)
    {
        var q = db.Exames.AsNoTracking();
        if (!incluirExcluidos) q = q.Where(e => e.ExcluidoEm == null);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var digitos = new string(termo.Where(char.IsDigit).ToArray());
            q = q.Where(e =>
                e.Paciente.Nome.Contains(termo)
                || e.ExameCatalogo.Nome.Contains(termo)
                || e.NomeMedico.Contains(termo)
                || (digitos.Length > 0 && e.Paciente.NumeroDocumento.Contains(digitos)));
        }

        return await q.OrderByDescending(e => e.DataEntrada).ThenByDescending(e => e.CriadoEm).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Exame>> ListarPorPacienteAsync(Guid pacienteId, bool incluirExcluidos, CancellationToken ct = default)
    {
        var q = db.Exames.AsNoTracking().Where(e => e.PacienteId == pacienteId);
        if (!incluirExcluidos) q = q.Where(e => e.ExcluidoEm == null);
        return await q.OrderByDescending(e => e.DataEntrada).ThenByDescending(e => e.CriadoEm).ToListAsync(ct);
    }

    public Task<bool> PacientePossuiExamesAtivosAsync(Guid pacienteId, CancellationToken ct = default)
        => db.Exames.AnyAsync(e => e.PacienteId == pacienteId && e.ExcluidoEm == null, ct);

    public async Task AdicionarAsync(Exame exame, CancellationToken ct = default)
    {
        db.Exames.Add(exame);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Exame exame, CancellationToken ct = default)
    {
        if (db.Entry(exame).State == EntityState.Detached)
            db.Exames.Attach(exame).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

public sealed class ExameCatalogoRepositorio(CligenDbContext db) : IExameCatalogoRepositorio
{
    public Task<ExameCatalogo?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.ExamesCatalogo.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExisteNomeAsync(string nome, Guid? ignorarId = null, CancellationToken ct = default)
        => db.ExamesCatalogo.AnyAsync(e => e.Nome == nome && (ignorarId == null || e.Id != ignorarId), ct);

    public async Task<IReadOnlyList<ExameCatalogo>> ListarAsync(bool somenteAtivos, CancellationToken ct = default)
    {
        var q = db.ExamesCatalogo.AsNoTracking();
        if (somenteAtivos) q = q.Where(e => e.Ativo);
        return await q.OrderBy(e => e.Nome).ToListAsync(ct);
    }

    public async Task AdicionarAsync(ExameCatalogo item, CancellationToken ct = default)
    {
        db.ExamesCatalogo.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(ExameCatalogo item, CancellationToken ct = default)
    {
        if (db.Entry(item).State == EntityState.Detached)
            db.ExamesCatalogo.Attach(item).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

public sealed class ParametroRepositorio(CligenDbContext db) : IParametroRepositorio
{
    public Task<Parametro?> ObterAsync(string chave, CancellationToken ct = default)
        => db.Parametros.FirstOrDefaultAsync(p => p.Chave == chave, ct);

    public async Task<IReadOnlyList<Parametro>> ListarAsync(CancellationToken ct = default)
        => await db.Parametros.AsNoTracking().OrderBy(p => p.Chave).ToListAsync(ct);

    public async Task AtualizarAsync(Parametro parametro, CancellationToken ct = default)
    {
        if (db.Entry(parametro).State == EntityState.Detached)
            db.Parametros.Attach(parametro).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

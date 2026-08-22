using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class PacienteRepositorio(CligenDbContext db) : IPacienteRepositorio
{
    public Task<Paciente?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Pacientes.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExisteDocumentoAsync(TipoDocumento tipo, string numero, Guid? ignorarId = null, CancellationToken ct = default)
        => db.Pacientes.AnyAsync(
            p => p.TipoDocumento == tipo && p.NumeroDocumento == numero && (ignorarId == null || p.Id != ignorarId), ct);

    public async Task<IReadOnlyList<Paciente>> ListarAsync(string? busca, bool incluirExcluidos, CancellationToken ct = default)
    {
        var q = db.Pacientes.AsNoTracking();

        if (!incluirExcluidos)
            q = q.Where(p => p.ExcluidoEm == null);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var digitos = new string(termo.Where(char.IsDigit).ToArray());
            q = q.Where(p =>
                p.Nome.Contains(termo)
                || p.Email.Contains(termo)
                || p.NumeroDocumento.Contains(termo.ToUpper())
                || (digitos.Length > 0 && (p.NumeroDocumento.Contains(digitos) || p.Telefone.Contains(digitos))));
        }

        return await q.OrderBy(p => p.Nome).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Paciente paciente, CancellationToken ct = default)
    {
        db.Pacientes.Add(paciente);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Paciente paciente, CancellationToken ct = default)
    {
        // Entidade rastreada desde ObterPorId; o change tracker detecta inclusive a criação/remoção do responsável.
        if (db.Entry(paciente).State == EntityState.Detached)
            db.Pacientes.Attach(paciente).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

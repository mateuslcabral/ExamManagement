using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Cligen.Infraestrutura.Persistencia.Repositorios;

public sealed class PacienteRepositorio(CligenDbContext db) : IPacienteRepositorio
{
    public Task<Paciente?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Pacientes.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExisteDocumentoAsync(Documento documento, Guid? ignorarId = null, CancellationToken ct = default)
        => db.Pacientes.AnyAsync(p =>
            p.TipoDocumento == documento.Tipo && p.NumeroDocumento == documento.Numero
            && (ignorarId == null || p.Id != ignorarId), ct);

    public async Task<(IReadOnlyList<Paciente> Itens, int Total)> BuscarAsync(
        string? busca, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var consulta = db.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            // Documento é gravado sem pontuação: "123.456" busca por "123456".
            var documento = new string(busca.Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant();
            var buscarDocumento = documento.Length >= 3;
            consulta = consulta.Where(p =>
                p.Nome.Contains(busca) || (buscarDocumento && p.NumeroDocumento.StartsWith(documento)));
        }

        var total = await consulta.CountAsync(ct);
        var itens = await consulta
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(ct);
        return (itens, total);
    }

    public async Task AdicionarAsync(Paciente paciente, CancellationToken ct = default)
    {
        db.Pacientes.Add(paciente);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Paciente paciente, CancellationToken ct = default)
    {
        // A entidade vem rastreada de ObterPorIdAsync; o change tracker cuida também da troca do responsável legal.
        if (db.Entry(paciente).State == EntityState.Detached)
            db.Pacientes.Attach(paciente).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);
    }
}

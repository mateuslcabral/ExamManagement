using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;
using Cligen.Dominio.ValueObjects;

namespace Cligen.Aplicacao.Servicos;

public sealed class PacienteService(
    IPacienteRepositorio repositorio,
    IEnvioEmail envioEmail,
    IEnvioWhatsApp envioWhatsApp,
    IExameRepositorio exames,
    IRelogio relogio)
{
    public async Task<IReadOnlyList<PacienteResumoDto>> ListarAsync(string? busca, bool incluirExcluidos, CancellationToken ct = default)
    {
        var hoje = relogio.Hoje;
        return (await repositorio.ListarAsync(busca, incluirExcluidos, ct)).Select(p => MapearResumo(p, hoje)).ToList();
    }

    public async Task<PacienteDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var p = await repositorio.ObterPorIdAsync(id, ct);
        return p is null ? null : Mapear(p, relogio.Hoje);
    }

    /// <summary>Cadastro pela equipe (D1). Dispara boas-vindas automaticamente (Q32).</summary>
    public async Task<PacienteDto> CriarAsync(SalvarPacienteRequest req, CancellationToken ct = default)
    {
        var hoje = relogio.Hoje;
        var paciente = Paciente.Criar(
            req.Nome, req.DataNascimento, req.TipoDocumento, req.NumeroDocumento,
            req.Email, req.Telefone, MapearResponsavel(req.ResponsavelLegal), hoje);

        await GarantirDocumentoUnicoAsync(paciente, null, ct);
        await repositorio.AdicionarAsync(paciente, ct);

        // Provedores ainda em aberto (Q36/Q37): as implementações atuais registram em log.
        await envioEmail.EnviarBoasVindasPacienteAsync(paciente.Email, paciente.Nome, ct);
        await envioWhatsApp.EnviarBoasVindasAsync(paciente.Telefone, paciente.Nome, ct);

        return Mapear(paciente, hoje);
    }

    public async Task<PacienteDto> AtualizarAsync(Guid id, SalvarPacienteRequest req, CancellationToken ct = default)
    {
        var hoje = relogio.Hoje;
        var paciente = await ObterOuFalharAsync(id, ct);

        paciente.Atualizar(
            req.Nome, req.DataNascimento, req.TipoDocumento, req.NumeroDocumento,
            req.Email, req.Telefone, MapearResponsavel(req.ResponsavelLegal), hoje);

        await GarantirDocumentoUnicoAsync(paciente, id, ct);
        await repositorio.AtualizarAsync(paciente, ct);
        return Mapear(paciente, hoje);
    }

    /// <summary>Exclusão lógica (P16). Bloqueada enquanto houver exame ativo vinculado (P25).</summary>
    public async Task ExcluirAsync(Guid id, ExcluirPacienteRequest req, CancellationToken ct = default)
    {
        var paciente = await ObterOuFalharAsync(id, ct);
        if (await exames.PacientePossuiExamesAtivosAsync(id, ct))
            throw new PacienteComExamesAtivosException();
        paciente.Excluir(req.UsuarioId, req.Motivo);
        await repositorio.AtualizarAsync(paciente, ct);
    }

    public async Task RestaurarAsync(Guid id, CancellationToken ct = default)
    {
        var paciente = await ObterOuFalharAsync(id, ct);
        paciente.Restaurar();
        await repositorio.AtualizarAsync(paciente, ct);
    }

    private async Task GarantirDocumentoUnicoAsync(Paciente paciente, Guid? ignorarId, CancellationToken ct)
    {
        if (await repositorio.ExisteDocumentoAsync(paciente.TipoDocumento, paciente.NumeroDocumento, ignorarId, ct))
            throw new DocumentoJaCadastradoException();
    }

    private async Task<Paciente> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new RegistroNaoEncontradoException("Paciente");

    private static Paciente.DadosResponsavel? MapearResponsavel(ResponsavelLegalRequest? r)
        => r is null ? null : new Paciente.DadosResponsavel(r.Nome, r.TipoDocumento, r.NumeroDocumento, r.Parentesco);

    private static PacienteDto Mapear(Paciente p, DateOnly hoje) => new(
        p.Id, p.Nome, p.DataNascimento, p.Idade(hoje), p.EhMenorDeIdade(hoje),
        p.TipoDocumento, p.NumeroDocumento, Documento.Formatar(p.TipoDocumento, p.NumeroDocumento),
        p.Email, p.Telefone,
        p.ResponsavelLegal is { } r
            ? new ResponsavelLegalDto(r.Nome, r.TipoDocumento, r.NumeroDocumento, Documento.Formatar(r.TipoDocumento, r.NumeroDocumento), r.Parentesco)
            : null,
        p.Excluido, p.ExcluidoEm, p.MotivoExclusao, p.CriadoEm, p.AtualizadoEm);

    private static PacienteResumoDto MapearResumo(Paciente p, DateOnly hoje) => new(
        p.Id, p.Nome, p.DataNascimento, p.Idade(hoje), p.EhMenorDeIdade(hoje),
        p.TipoDocumento, Documento.Formatar(p.TipoDocumento, p.NumeroDocumento),
        p.Email, p.Telefone, p.Excluido, p.CriadoEm);
}

using Cligen.Aplicacao.Comum;
using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Cadastro de pacientes (docs/05-regras-negocio/paciente.md). Sem exclusão: é prontuário.</summary>
public sealed class PacienteService(
    IPacienteRepositorio repositorio,
    IUsuarioAtual usuarioAtual,
    IEnvioEmail envioEmail,
    IEnvioWhatsApp envioWhatsApp,
    IUrlPortalPaciente urlPortal)
{
    public const int TamanhoPaginaMaximo = 100;

    public async Task<PaginaDto<PacienteDto>> BuscarAsync(string? busca, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, TamanhoPaginaMaximo);
        var hoje = Relogio.Hoje();

        var (itens, total) = await repositorio.BuscarAsync(busca?.Trim(), pagina, tamanhoPagina, ct);
        return new(itens.Select(p => Mapear(p, hoje)).ToList(), total, pagina, tamanhoPagina);
    }

    public async Task<PacienteDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var p = await repositorio.ObterPorIdAsync(id, ct);
        return p is null ? null : Mapear(p, Relogio.Hoje());
    }

    /// <summary>Cadastra e dispara a mensagem de boas-vindas por e-mail e WhatsApp (Q32).</summary>
    public async Task<PacienteDto> CriarAsync(SalvarPacienteRequest req, CancellationToken ct = default)
    {
        var documento = Documento.Criar(req.TipoDocumento, req.NumeroDocumento);
        await GarantirDocumentoDisponivelAsync(documento, null, ct);

        var hoje = Relogio.Hoje();
        var paciente = Paciente.Criar(
            req.Nome, req.DataNascimento, documento, req.Email, req.Telefone,
            CriarResponsavel(req.ResponsavelLegal), usuarioAtual.Id, hoje);
        await repositorio.AdicionarAsync(paciente, ct);

        // Envio síncrono enquanto não há fila com retry (jobs em background seguem pendentes).
        var boasVindas = new BoasVindasPaciente(
            paciente.Email, paciente.Telefone, paciente.ResponsavelLegal?.Nome ?? paciente.Nome, paciente.Nome, urlPortal.Obter());
        await envioEmail.EnviarBoasVindasPacienteAsync(boasVindas, ct);
        await envioWhatsApp.EnviarBoasVindasPacienteAsync(boasVindas, ct);

        return Mapear(paciente, hoje);
    }

    public async Task<PacienteDto> AtualizarAsync(Guid id, SalvarPacienteRequest req, CancellationToken ct = default)
    {
        var paciente = await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Paciente não encontrado.");

        var documento = Documento.Criar(req.TipoDocumento, req.NumeroDocumento);
        await GarantirDocumentoDisponivelAsync(documento, id, ct);

        var hoje = Relogio.Hoje();
        paciente.Atualizar(
            req.Nome, req.DataNascimento, documento, req.Email, req.Telefone,
            CriarResponsavel(req.ResponsavelLegal), usuarioAtual.Id, hoje);
        await repositorio.AtualizarAsync(paciente, ct);

        return Mapear(paciente, hoje);
    }

    private async Task GarantirDocumentoDisponivelAsync(Documento documento, Guid? ignorarId, CancellationToken ct)
    {
        if (await repositorio.ExisteDocumentoAsync(documento, ignorarId, ct))
            throw new DocumentoJaCadastradoException(documento.Descricao);
    }

    private static ResponsavelLegal? CriarResponsavel(ResponsavelLegalDto? dto)
        => dto is null
            ? null
            : ResponsavelLegal.Criar(dto.Nome, Documento.Criar(dto.TipoDocumento, dto.NumeroDocumento), dto.Parentesco);

    private static PacienteDto Mapear(Paciente p, DateOnly hoje) => new(
        p.Id, p.Nome, p.DataNascimento, p.EhMenorDeIdade(hoje), p.TipoDocumento, p.NumeroDocumento, p.Email, p.Telefone,
        p.ResponsavelLegal is { } r ? new(r.Nome, r.TipoDocumento, r.NumeroDocumento, r.Parentesco) : null,
        p.CriadoEm, p.AtualizadoEm);
}

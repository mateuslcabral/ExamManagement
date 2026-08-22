using System.Security.Cryptography;
using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

public sealed class ExameService(
    IExameRepositorio exames,
    IPacienteRepositorio pacientes,
    IExameCatalogoRepositorio catalogo,
    IArmazenamentoArquivos armazenamento,
    IRelogio relogio)
{
    public async Task<IReadOnlyList<ExameResumoDto>> ListarAsync(string? busca, bool incluirExcluidos, CancellationToken ct = default)
        => (await exames.ListarAsync(busca, incluirExcluidos, ct)).Select(MapearResumo).ToList();

    public async Task<IReadOnlyList<ExameResumoDto>> ListarPorPacienteAsync(Guid pacienteId, bool incluirExcluidos, CancellationToken ct = default)
        => (await exames.ListarPorPacienteAsync(pacienteId, incluirExcluidos, ct)).Select(MapearResumo).ToList();

    public async Task<ExameDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var e = await exames.ObterPorIdAsync(id, ct);
        return e is null ? null : Mapear(e);
    }

    public async Task<ExameDto> CriarAsync(CriarExameRequest req, CancellationToken ct = default)
    {
        var paciente = await pacientes.ObterPorIdAsync(req.PacienteId, ct) ?? throw new RegistroNaoEncontradoException("Paciente");
        var item = await catalogo.ObterPorIdAsync(req.ExameCatalogoId, ct) ?? throw new RegistroNaoEncontradoException("Exame do catálogo");

        var exame = Exame.Criar(paciente, item, req.Origem, req.Destino, req.TipoMedico, req.NomeMedico, req.Preco, relogio.Hoje);
        await exames.AdicionarAsync(exame, ct);
        return Mapear(exame);
    }

    public async Task<ExameDto> AtualizarAsync(Guid id, AtualizarExameRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Atualizar(req.Origem, req.Destino, req.TipoMedico, req.NomeMedico, req.Preco);
        await exames.AtualizarAsync(exame, ct);
        return Mapear(exame);
    }

    public async Task ExcluirAsync(Guid id, ExcluirExameRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Excluir(req.UsuarioId, req.Motivo);
        await exames.AtualizarAsync(exame, ct);
    }

    public async Task RestaurarAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Restaurar();
        await exames.AtualizarAsync(exame, ct);
    }

    // ---- Anexos ----

    public async Task<AnexoDto> AdicionarAnexoAsync(Guid exameId, string nomeOriginal, Stream conteudo, long tamanhoBytes, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(exameId, ct);

        // Validações de domínio antes de gravar em disco (limite de 3, formato, tamanho).
        if (exame.Excluido) throw new ExameExcluidoException();
        if (exame.Anexos.Count >= Exame.MaximoAnexos) throw new LimiteDeAnexosException();
        if (tamanhoBytes > Anexo.TamanhoMaximoBytes) throw new ValidacaoException("O arquivo excede o limite de 50 MB.");
        var extensao = Path.GetExtension(nomeOriginal);
        if (!Anexo.TiposPermitidos.ContainsKey(extensao))
            throw new ValidacaoException("Formato não permitido. Envie PDF, JPG ou PNG.");

        var caminhoRelativo = $"exames/{exame.Id}/{Guid.NewGuid()}{extensao.ToLowerInvariant()}";

        string hash;
        using (var sha = SHA256.Create())
            hash = Convert.ToHexString(await sha.ComputeHashAsync(conteudo, ct)).ToLowerInvariant();
        conteudo.Position = 0;

        var caminho = await armazenamento.SalvarAsync(caminhoRelativo, conteudo, ct);
        try
        {
            var anexo = exame.AdicionarAnexo(nomeOriginal, caminho, tamanhoBytes, hash);
            await exames.AtualizarAsync(exame, ct);
            return MapearAnexo(anexo);
        }
        catch
        {
            await armazenamento.RemoverAsync(caminho, CancellationToken.None);
            throw;
        }
    }

    public async Task<ArquivoAnexo> AbrirAnexoAsync(Guid exameId, Guid anexoId, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(exameId, ct);
        var anexo = exame.Anexos.FirstOrDefault(a => a.Id == anexoId) ?? throw new RegistroNaoEncontradoException("Anexo");
        var stream = await armazenamento.AbrirAsync(anexo.Caminho, ct);
        return new ArquivoAnexo(stream, anexo.NomeOriginal, anexo.TipoConteudo);
    }

    /// <summary>Remoção física (P27): o anexo foi enviado por engano; o exame em si nunca é apagado.</summary>
    public async Task RemoverAnexoAsync(Guid exameId, Guid anexoId, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(exameId, ct);
        var anexo = exame.RemoverAnexo(anexoId);
        await exames.AtualizarAsync(exame, ct);
        await armazenamento.RemoverAsync(anexo.Caminho, ct);
    }

    private async Task<Exame> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await exames.ObterPorIdAsync(id, ct) ?? throw new RegistroNaoEncontradoException("Exame");

    private static ExameDto Mapear(Exame e) => new(
        e.Id, e.PacienteId, e.Paciente.Nome, e.ExameCatalogoId, e.ExameCatalogo.Nome, e.ExameCatalogo.PrazoExecucaoDias,
        e.Origem, e.Destino, e.TipoMedico, e.NomeMedico, e.DataEntrada, e.Preco, e.Estado,
        e.DataLiberacaoPrevista, e.DataLiberacaoEfetiva,
        e.Anexos.OrderBy(a => a.EnviadoEm).Select(MapearAnexo).ToList(),
        e.AmostraAtiva is { } ativa ? MapearAmostra(ativa) : null,
        e.Amostras.OrderByDescending(a => a.CriadoEm).Select(MapearAmostra).ToList(),
        e.Etapas.OrderBy(et => et.Tipo).Select(MapearEtapa).ToList(),
        e.Excluido, e.ExcluidoEm, e.MotivoExclusao, e.CriadoEm, e.AtualizadoEm);

    private static ExameResumoDto MapearResumo(Exame e) => new(
        e.Id, e.PacienteId, e.Paciente.Nome, e.ExameCatalogo.Nome, e.Origem, e.TipoMedico, e.NomeMedico,
        e.DataEntrada, e.Preco, e.Estado, e.DataLiberacaoPrevista, e.Anexos.Count, e.Excluido);

    private static AnexoDto MapearAnexo(Anexo a) => new(a.Id, a.NomeOriginal, a.TipoConteudo, a.TamanhoBytes, a.EnviadoEm);

    internal static AmostraDto MapearAmostra(Amostra a) => new(a.Id, a.DataAcolhimento, a.Situacao, a.MotivoRejeicao, a.RejeitadaEm, a.CriadoEm);

    internal static EtapaLaudoDto MapearEtapa(EtapaAndamento e) => new(e.Tipo, e.Data, e.NomeOriginal, e.TamanhoBytes, e.ArquivoSubstituidoEm, e.Substituicoes);

    /// <summary>Exposto para os serviços de acolhimento e laudo reutilizarem o mapeamento completo.</summary>
    internal static ExameDto MapearCompleto(Exame e) => Mapear(e);
}

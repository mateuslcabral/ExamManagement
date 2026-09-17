using Cligen.Aplicacao.Comum;
using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Exame solicitado: cadastro, anexos e exclusão lógica (docs/05-regras-negocio/exame-solicitado.md).</summary>
public sealed class ExameService(
    IExameRepositorio repositorio,
    IPacienteRepositorio pacientes,
    IExameCatalogoRepositorio catalogo,
    ParametroService parametros,
    IArmazenamentoArquivos armazenamento,
    IUsuarioAtual usuarioAtual)
{
    public async Task<PaginaDto<ExameResumoDto>> BuscarAsync(
        string? busca, Guid? pacienteId, EstadoExame? estado, bool excluidos, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, PacienteService.TamanhoPaginaMaximo);
        var (itens, total) = await repositorio.BuscarAsync(busca?.Trim(), pacienteId, estado, excluidos, pagina, tamanhoPagina, ct);
        return new(itens, total, pagina, tamanhoPagina);
    }

    public async Task<ExameDto?> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await repositorio.ObterPorIdAsync(id, ct);
        return exame is null ? null : await MapearAsync(exame, ct);
    }

    public async Task<ExameDto> CriarAsync(SalvarExameRequest req, CancellationToken ct = default)
    {
        _ = await pacientes.ObterPorIdAsync(req.PacienteId, ct) ?? throw new ValidacaoException("Paciente não encontrado.");
        var itemCatalogo = await ObterCatalogoOuFalharAsync(req.ExameCatalogoId, ct);

        var exame = Exame.Criar(
            req.PacienteId, itemCatalogo, req.Origem, req.Destino, req.TipoMedico, req.NomeMedicoExterno, req.Preco,
            usuarioAtual.Id, Relogio.Hoje());
        await repositorio.AdicionarAsync(exame, ct);
        return await MapearAsync(exame, ct);
    }

    /// <summary>O paciente não muda: exame lançado no paciente errado se exclui (com motivo) e se cadastra de novo.</summary>
    public async Task<ExameDto> AtualizarAsync(Guid id, SalvarExameRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        if (req.PacienteId != exame.PacienteId)
            throw new ValidacaoException("O paciente do exame não pode ser alterado. Exclua o exame e cadastre-o no paciente correto.");
        if (req.Preco is null)
            throw new ValidacaoException("Informe o preço.");

        var itemCatalogo = await ObterCatalogoOuFalharAsync(req.ExameCatalogoId, ct);
        exame.Atualizar(itemCatalogo, req.Origem, req.Destino, req.TipoMedico, req.NomeMedicoExterno, req.Preco.Value, usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
        return await MapearAsync(exame, ct);
    }

    public async Task ExcluirAsync(Guid id, ExcluirExameRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.Excluir(req.Motivo, usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
    }

    /// <summary>Desfaz a exclusão lógica (C2: reversível).</summary>
    public async Task<ExameDto> RestaurarAsync(Guid id, CancellationToken ct = default)
    {
        var exame = await repositorio.ObterExcluidoPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame excluído não encontrado.");
        exame.Restaurar(usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
        return await MapearAsync(exame, ct);
    }

    /// <summary>Acolhimento: o prazo vem do catálogo e os dias de revisão do parâmetro vigentes neste momento.</summary>
    public async Task<ExameDto> AcolherAmostraAsync(Guid id, AcolherAmostraRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        var itemCatalogo = await ObterCatalogoOuFalharAsync(exame.ExameCatalogoId, ct);
        var diasRevisao = (await parametros.ObterDiasRevisaoAsync(ct)).Dias;

        exame.AcolherAmostra(req.DataAcolhimento, itemCatalogo, diasRevisao, usuarioAtual.Id, Relogio.Hoje());
        await repositorio.AtualizarAsync(exame, ct);
        return await MapearAsync(exame, ct);
    }

    public async Task<ExameDto> RejeitarAmostraAsync(Guid id, RejeitarAmostraRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.RejeitarAmostra(req.Motivo, usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
        return await MapearAsync(exame, ct);
    }

    /// <param name="conteudo">Precisa permitir Seek: o formato é conferido pelos primeiros bytes antes de gravar.</param>
    public async Task<AnexoDto> AdicionarAnexoAsync(
        Guid exameId, Stream conteudo, string? nomeOriginal, long tamanhoBytes, CancellationToken ct = default)
    {
        if (!conteudo.CanSeek)
            throw new ArgumentException("O conteúdo do anexo precisa permitir Seek.", nameof(conteudo));

        var exame = await ObterOuFalharAsync(exameId, ct);
        exame.GarantirPodeReceberAnexo();

        var cabecalho = new byte[FormatoArquivo.BytesCabecalho];
        var lidos = await conteudo.ReadAtLeastAsync(cabecalho, cabecalho.Length, throwOnEndOfStream: false, ct);
        conteudo.Position = 0;
        var formato = FormatoArquivo.Detectar(cabecalho.AsSpan(0, lidos), tamanhoBytes);

        // Se a gravação no banco falhar depois daqui, o binário fica sem referência — inofensivo, e nada é apagado.
        var armazenado = await armazenamento.SalvarAsync(conteudo, formato.Extensao, ct);
        var anexo = Anexo.Criar(nomeOriginal, formato, armazenado.TamanhoBytes, armazenado.HashSha256, armazenado.Caminho, usuarioAtual.Id);
        exame.AdicionarAnexo(anexo);
        await repositorio.AtualizarAsync(exame, ct);
        return Mapear(anexo);
    }

    public async Task RemoverAnexoAsync(Guid exameId, Guid anexoId, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(exameId, ct);
        exame.RemoverAnexo(anexoId, usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
    }

    /// <returns>Nulo se o exame ou o anexo (ativo) não existir.</returns>
    public async Task<(AnexoDto Anexo, Stream Conteudo)?> AbrirAnexoAsync(Guid exameId, Guid anexoId, CancellationToken ct = default)
    {
        var exame = await repositorio.ObterPorIdAsync(exameId, ct);
        var anexo = exame?.Anexos.FirstOrDefault(a => a.Id == anexoId && a.Ativo);
        if (anexo is null) return null;
        return (Mapear(anexo), await armazenamento.AbrirLeituraAsync(anexo.Caminho, ct));
    }

    private async Task<Exame> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame não encontrado.");

    private async Task<ExameCatalogo> ObterCatalogoOuFalharAsync(Guid id, CancellationToken ct)
        => await catalogo.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame não encontrado no catálogo.");

    internal async Task<ExameDto> MapearAsync(Exame e, CancellationToken ct)
    {
        var paciente = await pacientes.ObterPorIdAsync(e.PacienteId, ct)
                       ?? throw new InvalidOperationException($"Paciente {e.PacienteId} do exame {e.Id} não existe.");
        var itemCatalogo = await catalogo.ObterPorIdAsync(e.ExameCatalogoId, ct)
                           ?? throw new InvalidOperationException($"Catálogo {e.ExameCatalogoId} do exame {e.Id} não existe.");
        var diasRevisao = (await parametros.ObterDiasRevisaoAsync(ct)).Dias;

        return new ExameDto(
            e.Id, paciente.Id, paciente.Nome, paciente.TipoDocumento, paciente.NumeroDocumento,
            itemCatalogo.Id, itemCatalogo.Nome, itemCatalogo.PrazoExecucaoDias, itemCatalogo.PrazoEntregaDias(diasRevisao),
            e.Origem, e.Destino, e.TipoMedico, e.NomeMedico, e.Preco, e.DataEntrada, e.Estado, e.DataLiberacaoPrevista,
            e.DataLiberacaoEfetiva,
            e.Amostras.OrderByDescending(a => a.RegistradoEm).Select(Mapear).ToList(),
            e.Etapas.OrderBy(t => t.Tipo).Select(Mapear).ToList(),
            e.Anexos.Where(a => a.Ativo).OrderBy(a => a.EnviadoEm).Select(Mapear).ToList(),
            e.CriadoEm, e.AtualizadoEm);
    }

    private static AmostraDto Mapear(Amostra a) => new(
        a.Id, a.DataAcolhimento, a.PrazoExecucaoDias, a.DiasRevisao, a.DataLiberacaoPrevista, a.Recoleta,
        a.RegistradoEm, a.RejeitadaEm, a.MotivoRejeicao);

    internal static EtapaLaudoDto Mapear(EtapaAndamento t) => new(
        t.Id, t.Tipo, t.Data, t.NomeOriginal, t.TamanhoBytes, t.HashSha256, t.Substituicoes, t.SubstituidoEm);

    private static AnexoDto Mapear(Anexo a) => new(a.Id, a.NomeOriginal, a.TipoConteudo, a.TamanhoBytes, a.HashSha256, a.EnviadoEm);
}

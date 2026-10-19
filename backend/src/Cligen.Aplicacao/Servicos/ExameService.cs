using Cligen.Aplicacao.Comum;
using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Comum;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Exame solicitado: cadastro, anexos, comentários, amostra e exclusão lógica (docs/05-regras-negocio/exame-solicitado.md).</summary>
public sealed class ExameService(
    IExameRepositorio repositorio,
    IPacienteRepositorio pacientes,
    IExameCatalogoRepositorio catalogo,
    ICadastroRepositorio<Origem> origens,
    ICadastroRepositorio<Destino> destinos,
    IUsuarioRepositorio usuarios,
    IArmazenamentoArquivos armazenamento,
    IUsuarioAtual usuarioAtual)
{
    public async Task<PaginaDto<ExameResumoDto>> BuscarAsync(
        string? busca, Guid? pacienteId, EstadoExame? estado, bool excluidos,
        CampoDataExame? campoData, DateOnly? de, DateOnly? ate,
        int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, PacienteService.TamanhoPaginaMaximo);
        if (de is { } d && ate is { } a && a < d)
            throw new ValidacaoException("A data final não pode ser anterior à inicial.");

        var (itens, total) = await repositorio.BuscarAsync(busca?.Trim(), pacienteId, estado, excluidos, campoData, de, ate, pagina, tamanhoPagina, ct);
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
        await GarantirOrigemEDestinoAsync(req.OrigemId, req.DestinoId ?? itemCatalogo.DestinoId, ct);

        var exame = Exame.Criar(req.PacienteId, itemCatalogo, Dados(req), usuarioAtual.Id, Relogio.Hoje());
        await repositorio.AdicionarAsync(exame, ct);
        return await MapearAsync(exame, ct);
    }

    /// <summary>O paciente não muda: exame lançado no paciente errado se exclui (com motivo) e se cadastra de novo.</summary>
    public async Task<ExameDto> AtualizarAsync(Guid id, SalvarExameRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        if (req.PacienteId != exame.PacienteId)
            throw new ValidacaoException("O paciente do exame não pode ser alterado. Exclua o exame e cadastre-o no paciente correto.");

        var itemCatalogo = await ObterCatalogoOuFalharAsync(req.ExameCatalogoId, ct);
        // Origem/destino já gravados continuam válidos mesmo se forem desativados depois; só a troca exige item ativo.
        await GarantirOrigemEDestinoAsync(
            req.OrigemId == exame.OrigemId ? null : req.OrigemId,
            req.DestinoId == exame.DestinoId ? null : req.DestinoId, ct);

        exame.Atualizar(itemCatalogo, Dados(req), usuarioAtual.Id);
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

    public async Task<ExameDto> AdicionarComentarioAsync(Guid id, AdicionarComentarioRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        exame.AdicionarComentario(req.Texto, usuarioAtual.Id);
        await repositorio.AtualizarAsync(exame, ct);
        return await MapearAsync(exame, ct);
    }

    /// <summary>Acolhimento: o prazo vem do catálogo vigente neste momento e fica gravado na amostra.</summary>
    public async Task<ExameDto> AcolherAmostraAsync(Guid id, AcolherAmostraRequest req, CancellationToken ct = default)
    {
        var exame = await ObterOuFalharAsync(id, ct);
        var itemCatalogo = await ObterCatalogoOuFalharAsync(exame.ExameCatalogoId, ct);

        exame.AcolherAmostra(req.DataAcolhimento, itemCatalogo, usuarioAtual.Id, Relogio.Hoje());
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
        Guid exameId, TipoAnexo tipo, Stream conteudo, string? nomeOriginal, long tamanhoBytes, CancellationToken ct = default)
    {
        if (!conteudo.CanSeek)
            throw new ArgumentException("O conteúdo do anexo precisa permitir Seek.", nameof(conteudo));
        if (!Enum.IsDefined(tipo))
            throw new ValidacaoException("Informe o tipo do anexo: pedido médico, termo de consentimento ou outro.");

        var exame = await ObterOuFalharAsync(exameId, ct);
        exame.GarantirPodeReceberAnexo();

        var cabecalho = new byte[FormatoArquivo.BytesCabecalho];
        var lidos = await conteudo.ReadAtLeastAsync(cabecalho, cabecalho.Length, throwOnEndOfStream: false, ct);
        conteudo.Position = 0;
        var formato = FormatoArquivo.Detectar(cabecalho.AsSpan(0, lidos), tamanhoBytes);

        // Se a gravação no banco falhar depois daqui, o binário fica sem referência — inofensivo, e nada é apagado.
        var armazenado = await armazenamento.SalvarAsync(conteudo, formato.Extensao, ct);
        var anexo = Anexo.Criar(tipo, nomeOriginal, formato, armazenado.TamanhoBytes, armazenado.HashSha256, armazenado.Caminho, usuarioAtual.Id);
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

    private static DadosExame Dados(SalvarExameRequest r)
        => new(r.OrigemId, r.DestinoId, r.TipoMedico, r.NomeMedicoExterno, r.CrmMedico, r.UfCrm, r.Preco);

    /// <param name="origemId">Nulo não valida (origem não mudou).</param>
    private async Task GarantirOrigemEDestinoAsync(Guid? origemId, Guid? destinoId, CancellationToken ct)
    {
        if (origemId is { } o)
        {
            var origem = await origens.ObterPorIdAsync(o, ct) ?? throw new ValidacaoException("Origem não encontrada.");
            if (!origem.Ativo) throw new ValidacaoException($"A origem '{origem.Nome}' está inativa.");
        }
        if (destinoId is { } d)
        {
            var destino = await destinos.ObterPorIdAsync(d, ct) ?? throw new ValidacaoException("Destino não encontrado.");
            if (!destino.Ativo) throw new ValidacaoException($"O destino '{destino.Nome}' está inativo.");
        }
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
        var origem = await origens.ObterPorIdAsync(e.OrigemId, ct)
                     ?? throw new InvalidOperationException($"Origem {e.OrigemId} do exame {e.Id} não existe.");
        var destino = e.DestinoId is { } d ? await destinos.ObterPorIdAsync(d, ct) : null;
        var autores = await usuarios.ObterNomesAsync(e.Comentarios.Select(c => c.AutorId).Distinct(), ct);

        return new ExameDto(
            e.Id, e.Codigo, paciente.Id, paciente.Nome, paciente.TipoDocumento, paciente.NumeroDocumento,
            itemCatalogo.Id, itemCatalogo.Nome, itemCatalogo.PrazoDias, itemCatalogo.PrecoLimite,
            origem.Id, origem.Nome, destino?.Id, destino?.Nome,
            e.TipoMedico, e.NomeMedico, e.CrmMedico, e.UfCrm,
            e.Preco, e.Desconto, e.MotivoDesconto, e.ValorAPagar,
            e.DataEntrada, e.Estado, e.DataAcolhimento, e.DataLiberacaoPrevista, e.DataLiberacaoEfetiva,
            e.Amostras.OrderByDescending(a => a.RegistradoEm).Select(Mapear).ToList(),
            e.Etapas.OrderBy(t => t.Tipo).Select(Mapear).ToList(),
            e.Anexos.Where(a => a.Ativo).OrderBy(a => a.EnviadoEm).Select(Mapear).ToList(),
            e.Comentarios.OrderByDescending(c => c.CriadoEm)
                .Select(c => new ComentarioDto(c.Id, c.Texto, c.AutorId, autores.GetValueOrDefault(c.AutorId, "(usuário removido)"), c.CriadoEm))
                .ToList(),
            e.CriadoEm, e.AtualizadoEm);
    }

    private static AmostraDto Mapear(Amostra a) => new(
        a.Id, a.DataAcolhimento, a.PrazoDias, a.DataLiberacaoPrevista, a.Recoleta, a.RegistradoEm, a.RejeitadaEm, a.MotivoRejeicao);

    internal static EtapaLaudoDto Mapear(EtapaAndamento t) => new(
        t.Id, t.Tipo, t.Data, t.NomeOriginal, t.TamanhoBytes, t.HashSha256, t.Substituicoes, t.SubstituidoEm);

    private static AnexoDto Mapear(Anexo a) => new(a.Id, a.Tipo, a.NomeOriginal, a.TipoConteudo, a.TamanhoBytes, a.HashSha256, a.EnviadoEm);
}

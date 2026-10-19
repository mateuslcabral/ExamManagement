using Cligen.Aplicacao.Comum;
using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Financeiro (docs/05-regras-negocio/financeiro.md): lançamentos por exame e relatório por regime de caixa.</summary>
public sealed class PagamentoService(
    IPagamentoRepositorio repositorio,
    IExameRepositorio exames,
    IExameCatalogoRepositorio catalogo,
    ICadastroRepositorio<Convenio> convenios,
    IUsuarioAtual usuarioAtual)
{
    public async Task<FinanceiroExameDto> ResumoDoExameAsync(Guid exameId, CancellationToken ct = default)
    {
        var (exame, item) = await ObterExameECatalogoAsync(exameId, ct);
        return await ResumoAsync(exame, item, await repositorio.ListarPorExameAsync(exameId, ct), ct);
    }

    /// <summary>
    /// Lançamento manual (Q31). O total lançado (recebido + a receber) não pode passar do valor a pagar:
    /// não existe saldo em aberto do paciente (Q26), e lançar a mais seria erro de digitação.
    /// </summary>
    public async Task<FinanceiroExameDto> LancarAsync(Guid exameId, LancarPagamentoRequest req, CancellationToken ct = default)
    {
        var (exame, item) = await ObterExameECatalogoAsync(exameId, ct);
        var existentes = await repositorio.ListarPorExameAsync(exameId, ct);

        if (req.Metodo == MetodoPagamento.Convenio)
        {
            var convenio = await convenios.ObterPorIdAsync(req.ConvenioId ?? Guid.Empty, ct)
                           ?? throw new ValidacaoException("Informe o convênio.");
            if (!convenio.Ativo) throw new ValidacaoException($"O convênio '{convenio.Nome}' está inativo.");
        }

        var pagamento = Pagamento.Lancar(
            exameId, req.Metodo, req.Valor, req.DataRecebimento, req.ConvenioId, req.NumeroGuia,
            req.Parcelas, req.ValorParcela, req.Observacao, usuarioAtual.Id, Relogio.Hoje());

        var lancado = existentes.Where(p => p.ContaComoRecebido || p.ContaComoAReceber).Sum(p => p.Valor);
        if (lancado + pagamento.Valor > exame.ValorAPagar)
            throw new ValidacaoException(
                $"O total lançado ({Moeda(lancado + pagamento.Valor)}) passaria do valor a pagar ({Moeda(exame.ValorAPagar)}). Ajuste o valor ou o desconto.");

        await repositorio.AdicionarAsync(pagamento, ct);
        return await ResumoAsync(exame, item, [.. existentes, pagamento], ct);
    }

    public async Task<FinanceiroExameDto> MarcarRecebidoAsync(Guid pagamentoId, MarcarRecebidoRequest req, CancellationToken ct = default)
    {
        var pagamento = await ObterOuFalharAsync(pagamentoId, ct);
        pagamento.MarcarRecebido(req.DataRecebimento, usuarioAtual.Id, Relogio.Hoje());
        await repositorio.AtualizarAsync(pagamento, ct);
        return await ResumoDoExameAsync(pagamento.ExameId, ct);
    }

    public async Task<FinanceiroExameDto> GlosarAsync(Guid pagamentoId, MotivoRequest req, CancellationToken ct = default)
    {
        var pagamento = await ObterOuFalharAsync(pagamentoId, ct);
        pagamento.Glosar(req.Motivo, usuarioAtual.Id);
        await repositorio.AtualizarAsync(pagamento, ct);
        return await ResumoDoExameAsync(pagamento.ExameId, ct);
    }

    public async Task<FinanceiroExameDto> ExcluirAsync(Guid pagamentoId, MotivoRequest req, CancellationToken ct = default)
    {
        var pagamento = await ObterOuFalharAsync(pagamentoId, ct);
        pagamento.Excluir(req.Motivo, usuarioAtual.Id);
        await repositorio.AtualizarAsync(pagamento, ct);
        return await ResumoDoExameAsync(pagamento.ExameId, ct);
    }

    /// <summary>
    /// Desconto (C5) pode ser aplicado ou alterado enquanto o lançado não exceder o novo valor a pagar e o valor a
    /// pagar não ficar abaixo do preço limite do catálogo.
    /// </summary>
    public async Task<FinanceiroExameDto> AplicarDescontoAsync(Guid exameId, AplicarDescontoRequest req, CancellationToken ct = default)
    {
        var (exame, item) = await ObterExameECatalogoAsync(exameId, ct);
        var pagamentos = await repositorio.ListarPorExameAsync(exameId, ct);

        exame.AplicarDesconto(req.Desconto, req.Motivo, item.PrecoLimite, usuarioAtual.Id);
        var lancado = pagamentos.Where(p => p.ContaComoRecebido || p.ContaComoAReceber).Sum(p => p.Valor);
        if (lancado > exame.ValorAPagar)
            throw new ValidacaoException(
                $"Já há {Moeda(lancado)} lançados; o desconto deixaria o valor a pagar em {Moeda(exame.ValorAPagar)}. Exclua um lançamento antes.");

        await exames.AtualizarAsync(exame, ct);
        return await ResumoAsync(exame, item, pagamentos, ct);
    }

    public async Task<RelatorioFinanceiroDto> RelatorioAsync(DateOnly de, DateOnly ate, Guid? convenioId, Guid? origemId, CancellationToken ct = default)
    {
        if (ate < de) throw new ValidacaoException("A data final não pode ser anterior à inicial.");

        var recebidos = await repositorio.RecebidosNoPeriodoAsync(de, ate, convenioId, origemId, ct);
        var aReceber = await repositorio.AReceberAsync(convenioId, origemId, ct);

        return new RelatorioFinanceiroDto(
            de, ate, convenioId, origemId,
            recebidos.Sum(l => l.Valor),
            Totais(recebidos, l => l.Metodo.ToString()),
            Totais(recebidos, l => l.OrigemNome),
            recebidos,
            aReceber.Sum(l => l.Valor),
            Totais(aReceber, l => l.ConvenioNome ?? "(sem convênio)"),
            aReceber);
    }

    private static List<TotalPorChaveDto> Totais(IEnumerable<LinhaFinanceiraDto> linhas, Func<LinhaFinanceiraDto, string> chave)
        => linhas.GroupBy(chave)
            .Select(g => new TotalPorChaveDto(g.Key, g.Sum(l => l.Valor), g.Count()))
            .OrderByDescending(t => t.Total)
            .ToList();

    private async Task<FinanceiroExameDto> ResumoAsync(Exame exame, ExameCatalogo item, IReadOnlyList<Pagamento> pagamentos, CancellationToken ct)
    {
        var recebido = pagamentos.Where(p => p.ContaComoRecebido).Sum(p => p.Valor);
        var aReceber = pagamentos.Where(p => p.ContaComoAReceber).Sum(p => p.Valor);

        var idsConvenio = pagamentos.Select(p => p.ConvenioId).OfType<Guid>().Distinct().ToList();
        var nomes = idsConvenio.Count == 0
            ? new Dictionary<Guid, string>()
            : (await convenios.ListarAsync(false, ct)).Where(c => idsConvenio.Contains(c.Id)).ToDictionary(c => c.Id, c => c.Nome);

        return new FinanceiroExameDto(
            exame.Id, exame.Preco, exame.Desconto, exame.MotivoDesconto, item.PrecoLimite, exame.ValorAPagar,
            recebido, aReceber, exame.ValorAPagar - recebido - aReceber,
            pagamentos.Where(p => !p.Excluido).OrderBy(p => p.CriadoEm).Select(p => Mapear(p, nomes)).ToList());
    }

    private static PagamentoDto Mapear(Pagamento p, IReadOnlyDictionary<Guid, string> convenios) => new(
        p.Id, p.ExameId, p.Metodo, p.Valor, p.Status, p.DataRecebimento,
        p.ConvenioId, p.ConvenioId is { } c && convenios.TryGetValue(c, out var nome) ? nome : null,
        p.NumeroGuia, p.Parcelas, p.ValorParcela, p.Observacao, p.MotivoGlosa, p.CriadoEm);

    private async Task<Pagamento> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Lançamento não encontrado.");

    private async Task<(Exame Exame, ExameCatalogo Catalogo)> ObterExameECatalogoAsync(Guid id, CancellationToken ct)
    {
        var exame = await exames.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame não encontrado.");
        var item = await catalogo.ObterPorIdAsync(exame.ExameCatalogoId, ct)
                   ?? throw new InvalidOperationException($"Catálogo {exame.ExameCatalogoId} do exame {exame.Id} não existe.");
        return (exame, item);
    }

    private static string Moeda(decimal v) => "R$ " + v.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
}

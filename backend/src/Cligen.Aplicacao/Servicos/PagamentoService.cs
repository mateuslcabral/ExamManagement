using Cligen.Aplicacao.Comum;
using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Aplicacao.Servicos;

/// <summary>Financeiro (docs/05-regras-negocio/financeiro.md): lançamentos por exame e relatório por regime de caixa.</summary>
public sealed class PagamentoService(IPagamentoRepositorio repositorio, IExameRepositorio exames, IUsuarioAtual usuarioAtual)
{
    public async Task<FinanceiroExameDto> ResumoDoExameAsync(Guid exameId, CancellationToken ct = default)
    {
        var exame = await ObterExameOuFalharAsync(exameId, ct);
        return Resumo(exame, await repositorio.ListarPorExameAsync(exameId, ct));
    }

    /// <summary>
    /// Lançamento manual (Q31). O total lançado (recebido + a receber) não pode passar do valor a pagar:
    /// não existe saldo em aberto do paciente (Q26), e lançar a mais seria erro de digitação.
    /// </summary>
    public async Task<FinanceiroExameDto> LancarAsync(Guid exameId, LancarPagamentoRequest req, CancellationToken ct = default)
    {
        var exame = await ObterExameOuFalharAsync(exameId, ct);
        var existentes = await repositorio.ListarPorExameAsync(exameId, ct);

        var pagamento = Pagamento.Lancar(
            exameId, req.Metodo, req.Valor, req.DataRecebimento, req.Convenio, req.NumeroGuia,
            req.Parcelas, req.ValorParcela, req.Observacao, usuarioAtual.Id, Relogio.Hoje());

        var lancado = existentes.Where(p => p.ContaComoRecebido || p.ContaComoAReceber).Sum(p => p.Valor);
        if (lancado + pagamento.Valor > exame.ValorAPagar)
            throw new ValidacaoException(
                $"O total lançado ({Moeda(lancado + pagamento.Valor)}) passaria do valor a pagar ({Moeda(exame.ValorAPagar)}). Ajuste o valor ou o desconto.");

        await repositorio.AdicionarAsync(pagamento, ct);
        return Resumo(exame, [.. existentes, pagamento]);
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
    /// Desconto (C5) pode ser aplicado ou alterado enquanto o lançado não exceder o novo valor a pagar.
    /// </summary>
    public async Task<FinanceiroExameDto> AplicarDescontoAsync(Guid exameId, AplicarDescontoRequest req, CancellationToken ct = default)
    {
        var exame = await ObterExameOuFalharAsync(exameId, ct);
        var pagamentos = await repositorio.ListarPorExameAsync(exameId, ct);

        exame.AplicarDesconto(req.Desconto, req.Motivo, usuarioAtual.Id);
        var lancado = pagamentos.Where(p => p.ContaComoRecebido || p.ContaComoAReceber).Sum(p => p.Valor);
        if (lancado > exame.ValorAPagar)
            throw new ValidacaoException(
                $"Já há {Moeda(lancado)} lançados; o desconto deixaria o valor a pagar em {Moeda(exame.ValorAPagar)}. Exclua um lançamento antes.");

        await exames.AtualizarAsync(exame, ct);
        return Resumo(exame, pagamentos);
    }

    public async Task<RelatorioFinanceiroDto> RelatorioAsync(DateOnly de, DateOnly ate, CancellationToken ct = default)
    {
        if (ate < de) throw new ValidacaoException("A data final não pode ser anterior à inicial.");

        var recebidos = await repositorio.RecebidosNoPeriodoAsync(de, ate, ct);
        var aReceber = await repositorio.AReceberAsync(ct);

        return new RelatorioFinanceiroDto(
            de, ate,
            recebidos.Sum(l => l.Valor),
            Totais(recebidos, l => l.Metodo.ToString()),
            recebidos,
            aReceber.Sum(l => l.Valor),
            Totais(aReceber, l => l.Convenio ?? "(sem convênio)"),
            aReceber);
    }

    private static List<TotalPorChaveDto> Totais(IEnumerable<LinhaFinanceiraDto> linhas, Func<LinhaFinanceiraDto, string> chave)
        => linhas.GroupBy(chave)
            .Select(g => new TotalPorChaveDto(g.Key, g.Sum(l => l.Valor), g.Count()))
            .OrderByDescending(t => t.Total)
            .ToList();

    private static FinanceiroExameDto Resumo(Exame exame, IReadOnlyList<Pagamento> pagamentos)
    {
        var recebido = pagamentos.Where(p => p.ContaComoRecebido).Sum(p => p.Valor);
        var aReceber = pagamentos.Where(p => p.ContaComoAReceber).Sum(p => p.Valor);
        return new FinanceiroExameDto(
            exame.Id, exame.Preco, exame.Desconto, exame.MotivoDesconto, exame.ValorAPagar,
            recebido, aReceber, exame.ValorAPagar - recebido - aReceber,
            pagamentos.Where(p => !p.Excluido).OrderBy(p => p.CriadoEm).Select(Mapear).ToList());
    }

    private static PagamentoDto Mapear(Pagamento p) => new(
        p.Id, p.ExameId, p.Metodo, p.Valor, p.Status, p.DataRecebimento, p.Convenio, p.NumeroGuia,
        p.Parcelas, p.ValorParcela, p.Observacao, p.MotivoGlosa, p.CriadoEm);

    private async Task<Pagamento> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await repositorio.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Lançamento não encontrado.");

    private async Task<Exame> ObterExameOuFalharAsync(Guid id, CancellationToken ct)
        => await exames.ObterPorIdAsync(id, ct) ?? throw new ValidacaoException("Exame não encontrado.");

    private static string Moeda(decimal v) => "R$ " + v.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
}

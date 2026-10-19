using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Lançamento financeiro de um exame (docs/05-regras-negocio/financeiro.md). Agregado próprio: relatórios cruzam
/// pagamentos de vários exames. Nunca é apagado fisicamente (P4): lançamento errado é excluído logicamente, com motivo.
/// </summary>
public class Pagamento
{
    public const int TamanhoMaximoGuia = 50;
    public const int TamanhoMaximoObservacao = 300;
    public const int TamanhoMaximoMotivo = 500;
    public const int MaximoParcelas = 24;

    public Guid Id { get; private set; }
    public Guid ExameId { get; private set; }
    public MetodoPagamento Metodo { get; private set; }
    public decimal Valor { get; private set; }
    public StatusPagamento Status { get; private set; }

    /// <summary>Preenchida quando o status vira recebido; é a data que conta para o faturamento (Q29).</summary>
    public DateOnly? DataRecebimento { get; private set; }

    /// <summary>Operadora (cadastro — C4.5). Só para <see cref="MetodoPagamento.Convenio"/>.</summary>
    public Guid? ConvenioId { get; private set; }

    /// <summary>Número de guia/autorização, opcional (C4.2 pendente). Só para convênio.</summary>
    public string? NumeroGuia { get; private set; }

    /// <summary>Cartão parcelado (Q27): informação, não afeta status nem data (C6, P3). Só para cartão de crédito.</summary>
    public int? Parcelas { get; private set; }
    public decimal? ValorParcela { get; private set; }

    public string? Observacao { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public Guid CriadoPorId { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public Guid? AtualizadoPorId { get; private set; }

    public DateTime? ExcluidoEm { get; private set; }
    public Guid? ExcluidoPorId { get; private set; }
    public string? MotivoExclusao { get; private set; }
    public bool Excluido => ExcluidoEm is not null;

    /// <summary>Glosa só existe para convênio; para os demais, motivo da glosa fica nulo.</summary>
    public string? MotivoGlosa { get; private set; }

    private Pagamento() { } // EF Core

    /// <summary>
    /// Particular quita no ato (Q26): nasce <b>recebido</b> na data informada (hoje por padrão).
    /// Convênio nasce <b>a receber</b> (C4) e entra no caixa só quando o dinheiro chega.
    /// </summary>
    public static Pagamento Lancar(
        Guid exameId, MetodoPagamento metodo, decimal valor, DateOnly? dataRecebimento,
        Guid? convenioId, string? numeroGuia, int? parcelas, decimal? valorParcela, string? observacao,
        Guid autorId, DateOnly hoje)
    {
        if (exameId == Guid.Empty) throw new ValidacaoException("O exame é obrigatório.");
        if (!Enum.IsDefined(metodo)) throw new ValidacaoException("Método de pagamento inválido.");
        ValidarValor(valor, "O valor");

        var p = new Pagamento
        {
            Id = Guid.NewGuid(),
            ExameId = exameId,
            Metodo = metodo,
            Valor = valor,
            Observacao = Aparar(observacao, TamanhoMaximoObservacao, "A observação"),
            CriadoEm = DateTime.UtcNow,
            CriadoPorId = autorId
        };

        if (metodo == MetodoPagamento.Convenio)
        {
            p.ConvenioId = convenioId is { } id && id != Guid.Empty ? id : throw new ValidacaoException("Informe o convênio.");
            p.NumeroGuia = Aparar(numeroGuia, TamanhoMaximoGuia, "O número da guia");
            p.Status = StatusPagamento.AReceber;
        }
        else
        {
            p.Status = StatusPagamento.Recebido;
            p.DataRecebimento = ValidarDataRecebimento(dataRecebimento ?? hoje, hoje);
        }

        if (metodo == MetodoPagamento.CartaoCredito && parcelas is > 1)
        {
            if (parcelas > MaximoParcelas)
                throw new ValidacaoException($"No máximo {MaximoParcelas} parcelas.");
            p.Parcelas = parcelas;
            p.ValorParcela = valorParcela is { } vp
                ? ValidarValor(vp, "O valor da parcela")
                : decimal.Round(valor / parcelas.Value, 2, MidpointRounding.AwayFromZero);
        }

        return p;
    }

    /// <summary>Convênio pagou (C4). Só de <see cref="StatusPagamento.AReceber"/>.</summary>
    public void MarcarRecebido(DateOnly dataRecebimento, Guid autorId, DateOnly hoje)
    {
        GarantirNaoExcluido();
        if (Status != StatusPagamento.AReceber)
            throw new ValidacaoException("Só um lançamento a receber pode ser marcado como recebido.");
        DataRecebimento = ValidarDataRecebimento(dataRecebimento, hoje);
        Status = StatusPagamento.Recebido;
        Tocar(autorId);
    }

    /// <summary>Recusa do convênio (P11): sai da previsão, lançamento permanece.</summary>
    public void Glosar(string? motivo, Guid autorId)
    {
        GarantirNaoExcluido();
        if (Metodo != MetodoPagamento.Convenio)
            throw new ValidacaoException("Glosa só se aplica a lançamento de convênio.");
        if (Status != StatusPagamento.AReceber)
            throw new ValidacaoException("Só um lançamento a receber pode ser glosado.");
        MotivoGlosa = Aparar(motivo, TamanhoMaximoMotivo, "O motivo") ?? throw new ValidacaoException("Informe o motivo da glosa.");
        Status = StatusPagamento.Glosado;
        Tocar(autorId);
    }

    /// <summary>Lançamento errado: exclusão lógica com motivo, mesmo padrão do exame (P4, P10).</summary>
    public void Excluir(string? motivo, Guid autorId)
    {
        GarantirNaoExcluido();
        MotivoExclusao = Aparar(motivo, TamanhoMaximoMotivo, "O motivo") ?? throw new ValidacaoException("Informe o motivo da exclusão.");
        ExcluidoEm = DateTime.UtcNow;
        ExcluidoPorId = autorId;
    }

    /// <summary>Entra no faturamento e no "recebido" do exame.</summary>
    public bool ContaComoRecebido => !Excluido && Status == StatusPagamento.Recebido;

    /// <summary>Entra na previsão (valor a receber de convênio).</summary>
    public bool ContaComoAReceber => !Excluido && Status == StatusPagamento.AReceber;

    private void GarantirNaoExcluido()
    {
        if (Excluido) throw new ValidacaoException("Este lançamento foi excluído.");
    }

    private void Tocar(Guid autorId)
    {
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPorId = autorId;
    }

    private static decimal ValidarValor(decimal valor, string rotulo)
    {
        if (valor <= 0) throw new ValidacaoException($"{rotulo} deve ser maior que zero.");
        if (decimal.Round(valor, 2) != valor) throw new ValidacaoException($"{rotulo} deve ter no máximo 2 casas decimais.");
        return valor;
    }

    private static DateOnly ValidarDataRecebimento(DateOnly data, DateOnly hoje)
    {
        if (data > hoje) throw new ValidacaoException("A data de recebimento não pode estar no futuro.");
        return data;
    }

    private static string? Aparar(string? texto, int maximo, string rotulo)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        texto = texto.Trim();
        if (texto.Length > maximo) throw new ValidacaoException($"{rotulo} deve ter no máximo {maximo} caracteres.");
        return texto;
    }
}

using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>Campos editáveis do exame solicitado.</summary>
/// <param name="DestinoId">Nulo na criação usa o destino padrão do catálogo.</param>
/// <param name="Preco">Nulo na criação usa o preço de referência do catálogo.</param>
public sealed record DadosExame(
    Guid OrigemId,
    Guid? DestinoId,
    TipoMedico TipoMedico,
    string? NomeMedicoExterno,
    string? CrmMedico,
    string? UfCrm,
    decimal? Preco);

/// <summary>
/// Exame solicitado — registro de prontuário (Q45). Nasce aguardando amostra; o acolhimento e as etapas do laudo
/// fazem o estado avançar. Nunca é apagado fisicamente: a exclusão é lógica, com autor, data e motivo (C2, P4, P10).
/// </summary>
public class Exame
{
    /// <summary>Único médico interno (Q13); nome usado no modelo de mensagem de médico interno (D13).</summary>
    public const string NomeMedicoInterno = "Dr. Arsonval Lamounier Junior";
    public const int MaximoAnexos = 3; // Q22
    public const int TamanhoMaximoNomeMedico = 200;
    public const int TamanhoMaximoCrm = 20;
    public const int TamanhoMaximoMotivoExclusao = 500;
    public const int TamanhoMaximoMotivoDesconto = 300;

    public static readonly IReadOnlySet<string> Ufs = new HashSet<string>
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public Guid Id { get; private set; }

    /// <summary>Sequencial do banco; base do código interno legível.</summary>
    public long Numero { get; private set; }
    public string Codigo => $"EX-{Numero:D6}";

    public Guid PacienteId { get; private set; }
    public Guid ExameCatalogoId { get; private set; }
    public Guid OrigemId { get; private set; }
    public Guid? DestinoId { get; private set; }

    public TipoMedico TipoMedico { get; private set; }

    /// <summary>Digitado a cada exame, sem cadastro (Q14, Q15). Nulo para médico interno.</summary>
    public string? NomeMedicoExterno { get; private set; }
    public string? CrmMedico { get; private set; }
    public string? UfCrm { get; private set; }

    /// <summary>Herdado do preço de referência do catálogo e editável, respeitando o preço limite.</summary>
    public decimal Preco { get; private set; }

    /// <summary>Sempre em reais, nunca percentual (C5). Motivo livre e opcional (P12).</summary>
    public decimal Desconto { get; private set; }
    public string? MotivoDesconto { get; private set; }

    /// <summary>valor pago = valor do exame − desconto (C5).</summary>
    public decimal ValorAPagar => Preco - Desconto;

    public DateOnly DataEntrada { get; private set; }
    public EstadoExame Estado { get; private set; }

    /// <summary>Cópia da data da amostra vigente, para filtros das filas. Nula sem amostra ou após rejeição.</summary>
    public DateOnly? DataAcolhimento { get; private set; }

    /// <summary>
    /// Gravada uma única vez no acolhimento e nunca recalculada (P14): é a diferença para a data efetiva que torna
    /// o atraso mensurável. Nula enquanto não há amostra acolhida — inclusive após rejeição (Q19).
    /// </summary>
    public DateOnly? DataLiberacaoPrevista { get; private set; }

    /// <summary>Gravada ao disponibilizar ao paciente (D10). A diferença para a prevista mede o atraso (A7).</summary>
    public DateTime? DataLiberacaoEfetiva { get; private set; }

    private readonly List<Anexo> _anexos = [];
    public IReadOnlyCollection<Anexo> Anexos => _anexos.AsReadOnly();

    private readonly List<EtapaAndamento> _etapas = [];
    public IReadOnlyCollection<EtapaAndamento> Etapas => _etapas.AsReadOnly();
    public EtapaAndamento? Etapa(TipoEtapaLaudo tipo) => _etapas.FirstOrDefault(e => e.Tipo == tipo);

    private readonly List<Amostra> _amostras = [];
    /// <summary>Histórico completo: amostras rejeitadas permanecem.</summary>
    public IReadOnlyCollection<Amostra> Amostras => _amostras.AsReadOnly();

    /// <summary>Amostra acolhida e não rejeitada. Há no máximo uma (P8).</summary>
    public Amostra? AmostraVigente => _amostras.SingleOrDefault(a => !a.Rejeitada);

    private readonly List<Comentario> _comentarios = [];
    public IReadOnlyCollection<Comentario> Comentarios => _comentarios.AsReadOnly();

    public DateTime CriadoEm { get; private set; }
    public Guid CriadoPorId { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public Guid? AtualizadoPorId { get; private set; }

    public DateTime? ExcluidoEm { get; private set; }
    public Guid? ExcluidoPorId { get; private set; }
    public string? MotivoExclusao { get; private set; }

    public bool Excluido => ExcluidoEm is not null;
    public string NomeMedico => TipoMedico == TipoMedico.Interno ? NomeMedicoInterno : NomeMedicoExterno!;

    private Exame() { } // EF Core

    public static Exame Criar(Guid pacienteId, ExameCatalogo catalogo, DadosExame dados, Guid autorId, DateOnly hoje)
    {
        if (pacienteId == Guid.Empty)
            throw new ValidacaoException("O paciente é obrigatório.");
        if (!catalogo.Ativo)
            throw new ValidacaoException($"O exame '{catalogo.Nome}' está inativo no catálogo.");

        var exame = new Exame
        {
            Id = Guid.NewGuid(),
            PacienteId = pacienteId,
            ExameCatalogoId = catalogo.Id,
            DataEntrada = hoje,
            Estado = EstadoExame.AguardandoAmostra,
            CriadoEm = DateTime.UtcNow,
            CriadoPorId = autorId
        };
        exame.Aplicar(
            dados with { DestinoId = dados.DestinoId ?? catalogo.DestinoId, Preco = dados.Preco ?? catalogo.PrecoReferencia },
            catalogo);
        return exame;
    }

    public void Atualizar(ExameCatalogo catalogo, DadosExame dados, Guid autorId)
    {
        GarantirNaoExcluido();
        if (dados.Preco is null)
            throw new ValidacaoException("Informe o preço.");

        if (catalogo.Id != ExameCatalogoId)
        {
            // O prazo do catálogo alimenta a previsão gravada no acolhimento (P14): depois dele, trocar o exame
            // deixaria a data prevista incoerente com o exame registrado.
            if (Estado != EstadoExame.AguardandoAmostra)
                throw new ValidacaoException("O exame do catálogo só pode ser trocado antes do acolhimento da amostra.");
            if (!catalogo.Ativo)
                throw new ValidacaoException($"O exame '{catalogo.Nome}' está inativo no catálogo.");
        }

        Aplicar(dados, catalogo);
        ExameCatalogoId = catalogo.Id;
        Tocar(autorId);
    }

    // ---- Anexos e comentários ----

    public void AdicionarAnexo(Anexo anexo)
    {
        GarantirPodeReceberAnexo();
        _anexos.Add(anexo);
    }

    /// <summary>Permite checar antes de gravar o arquivo no armazenamento, evitando binário órfão.</summary>
    public void GarantirPodeReceberAnexo()
    {
        GarantirNaoExcluido();
        if (_anexos.Count(a => a.Ativo) >= MaximoAnexos)
            throw new ValidacaoException($"O exame já tem {MaximoAnexos} anexos. Remova um antes de enviar outro.");
    }

    public void RemoverAnexo(Guid anexoId, Guid autorId)
    {
        GarantirNaoExcluido();
        var anexo = _anexos.FirstOrDefault(a => a.Id == anexoId) ?? throw new ValidacaoException("Anexo não encontrado.");
        anexo.Remover(autorId);
    }

    /// <summary>Anotação da equipe; não se edita nem se apaga.</summary>
    public Comentario AdicionarComentario(string? texto, Guid autorId)
    {
        GarantirNaoExcluido();
        var comentario = Comentario.Criar(texto, autorId);
        _comentarios.Add(comentario);
        return comentario;
    }

    // ---- Amostra (docs/05-regras-negocio/amostra-e-prazo.md) ----

    /// <summary>
    /// Registra a chegada da amostra e fixa a previsão de liberação: acolhimento + prazo do catálogo (Q1). A data é
    /// digitável porque a amostra pode chegar num dia e ser lançada noutro (Q1.2 — a confirmar).
    /// </summary>
    /// <param name="catalogo">Item do catálogo deste exame, de onde vem o prazo atual.</param>
    /// <param name="hoje">Data local da clínica.</param>
    public void AcolherAmostra(DateOnly dataAcolhimento, ExameCatalogo catalogo, Guid autorId, DateOnly hoje)
    {
        GarantirNaoExcluido();
        if (catalogo.Id != ExameCatalogoId)
            throw new InvalidOperationException("O catálogo informado não é o deste exame.");
        if (Estado != EstadoExame.AguardandoAmostra)
            throw new ValidacaoException("A amostra deste exame já foi acolhida.");
        if (dataAcolhimento > hoje)
            throw new ValidacaoException("A data de acolhimento não pode estar no futuro.");
        if (dataAcolhimento < DataEntrada)
            throw new ValidacaoException("A data de acolhimento não pode ser anterior à entrada do exame.");

        var amostra = Amostra.Acolher(dataAcolhimento, catalogo.PrazoDias, recoleta: _amostras.Count > 0, autorId);
        _amostras.Add(amostra);
        DataAcolhimento = amostra.DataAcolhimento;
        DataLiberacaoPrevista = amostra.DataLiberacaoPrevista;
        Estado = EstadoExame.AmostraAcolhida;
    }

    /// <summary>
    /// Rejeição zera o prazo e o exame volta a aguardar amostra; a recoleta gera nova previsão (Q19 — leitura a
    /// confirmar em Q19.1). Só antes das etapas do laudo: depois delas a amostra já foi processada.
    /// </summary>
    public void RejeitarAmostra(string? motivo, Guid autorId)
    {
        GarantirNaoExcluido();
        if (Estado != EstadoExame.AmostraAcolhida)
            throw new ValidacaoException("Só é possível rejeitar a amostra enquanto ela está acolhida, antes das etapas do laudo.");

        AmostraVigente!.Rejeitar(motivo, autorId);
        DataAcolhimento = null;
        DataLiberacaoPrevista = null;
        Estado = EstadoExame.AguardandoAmostra;
    }

    // ---- Financeiro (docs/05-regras-negocio/financeiro.md) ----

    /// <summary>Desconto em reais (C5). Zero remove o desconto. O valor a pagar não pode ficar abaixo do preço limite.</summary>
    public void AplicarDesconto(decimal desconto, string? motivo, decimal precoLimite, Guid autorId)
    {
        GarantirNaoExcluido();
        if (desconto < 0)
            throw new ValidacaoException("O desconto não pode ser negativo.");
        if (decimal.Round(desconto, 2) != desconto)
            throw new ValidacaoException("O desconto deve ter no máximo 2 casas decimais.");
        if (desconto > Preco)
            throw new ValidacaoException("O desconto não pode ser maior que o preço do exame.");
        GarantirPiso(Preco - desconto, precoLimite);

        motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        if (motivo?.Length > TamanhoMaximoMotivoDesconto)
            throw new ValidacaoException($"O motivo do desconto deve ter no máximo {TamanhoMaximoMotivoDesconto} caracteres.");

        Desconto = desconto;
        MotivoDesconto = desconto == 0 ? null : motivo;
        Tocar(autorId);
    }

    // ---- Fluxo do laudo (docs/05-regras-negocio/fluxo-laudo.md) ----

    /// <summary>
    /// Permite conferir antes de gravar o arquivo no armazenamento. Uma etapa nova só pode ser registrada quando o
    /// exame está no estado imediatamente anterior (2→3→4→5, sem pular); uma etapa já registrada aceita substituição
    /// em qualquer estado posterior, inclusive após a disponibilização (R2: sem versão nem aviso ao paciente).
    /// </summary>
    public void GarantirPodeReceberEtapa(TipoEtapaLaudo tipo)
    {
        GarantirNaoExcluido();
        if (!Enum.IsDefined(tipo))
            throw new ValidacaoException("Etapa inválida.");
        if (Etapa(tipo) is null && Estado != (EstadoExame)((int)tipo - 1))
            throw new EstadoInvalidoException($"registrar '{DescreverEstado((EstadoExame)tipo)}'", DescreverEstado(Estado));
    }

    /// <summary>Registra a etapa (data automática, avança o estado) ou substitui o arquivo de etapa já registrada (Q20).</summary>
    /// <returns>true se foi substituição.</returns>
    public bool RegistrarOuSubstituirEtapa(TipoEtapaLaudo tipo, ArquivoLaudo arquivo, Guid autorId)
    {
        GarantirPodeReceberEtapa(tipo);

        if (Etapa(tipo) is { } existente)
        {
            existente.SubstituirArquivo(arquivo, autorId);
            return true;
        }

        _etapas.Add(EtapaAndamento.Registrar(tipo, arquivo, autorId));
        Estado = (EstadoExame)tipo;
        return false;
    }

    /// <summary>Ato manual (D10): só com o laudo revisado. Grava a data efetiva; a notificação é da Aplicação.</summary>
    public void Disponibilizar()
    {
        GarantirNaoExcluido();
        if (Estado != EstadoExame.LaudoRevisado)
            throw new EstadoInvalidoException("disponibilizar ao paciente", DescreverEstado(Estado));
        if (Etapa(TipoEtapaLaudo.LaudoRevisado) is null)
            throw new ValidacaoException("O laudo revisado não foi enviado.");

        DataLiberacaoEfetiva = DateTime.UtcNow;
        Estado = EstadoExame.Disponibilizado;
    }

    public static string DescreverEstado(EstadoExame estado) => estado switch
    {
        EstadoExame.AguardandoAmostra => "Aguardando amostra",
        EstadoExame.AmostraAcolhida => "Amostra acolhida",
        EstadoExame.LaudoParceiroPronto => "Laudo parceiro pronto",
        EstadoExame.LaudoCligenParaRevisao => "Laudo Cligen para revisão",
        EstadoExame.LaudoRevisado => "Laudo revisado",
        EstadoExame.Disponibilizado => "Disponibilizado ao paciente",
        _ => estado.ToString()
    };

    // ---- Exclusão lógica ----

    /// <summary>Qualquer funcionário, em qualquer etapa (Q18). Motivo obrigatório (P10).</summary>
    public void Excluir(string? motivo, Guid autorId)
    {
        GarantirNaoExcluido();
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ValidacaoException("Informe o motivo da exclusão.");
        motivo = motivo.Trim();
        if (motivo.Length > TamanhoMaximoMotivoExclusao)
            throw new ValidacaoException($"O motivo deve ter no máximo {TamanhoMaximoMotivoExclusao} caracteres.");

        ExcluidoEm = DateTime.UtcNow;
        ExcluidoPorId = autorId;
        MotivoExclusao = motivo;
    }

    /// <summary>A exclusão é reversível (C2). Autor, data e motivo da exclusão desfeita não são preservados na entidade.</summary>
    public void Restaurar(Guid autorId)
    {
        if (!Excluido) throw new ValidacaoException("Este exame não está excluído.");
        ExcluidoEm = null;
        ExcluidoPorId = null;
        MotivoExclusao = null;
        Tocar(autorId);
    }

    private void GarantirNaoExcluido()
    {
        if (Excluido) throw new ValidacaoException("Este exame foi excluído e não pode ser alterado.");
    }

    private void Tocar(Guid autorId)
    {
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPorId = autorId;
    }

    private static void GarantirPiso(decimal valorAPagar, decimal precoLimite)
    {
        if (valorAPagar < precoLimite)
            throw new ValidacaoException(
                $"O valor a pagar não pode ficar abaixo do preço limite do catálogo (R$ {precoLimite:N2}).");
    }

    private void Aplicar(DadosExame d, ExameCatalogo catalogo)
    {
        if (d.OrigemId == Guid.Empty)
            throw new ValidacaoException("A origem é obrigatória.");

        string? nomeMedicoExterno = null, crm = null, uf = null;
        switch (d.TipoMedico)
        {
            case TipoMedico.Interno:
                break;
            case TipoMedico.Externo:
                if (string.IsNullOrWhiteSpace(d.NomeMedicoExterno))
                    throw new ValidacaoException("Informe o nome do médico solicitante externo.");
                nomeMedicoExterno = d.NomeMedicoExterno.Trim();
                if (nomeMedicoExterno.Length > TamanhoMaximoNomeMedico)
                    throw new ValidacaoException($"O nome do médico deve ter no máximo {TamanhoMaximoNomeMedico} caracteres.");

                crm = string.IsNullOrWhiteSpace(d.CrmMedico) ? null : d.CrmMedico.Trim();
                if (crm?.Length > TamanhoMaximoCrm)
                    throw new ValidacaoException($"O CRM deve ter no máximo {TamanhoMaximoCrm} caracteres.");

                uf = string.IsNullOrWhiteSpace(d.UfCrm) ? null : d.UfCrm.Trim().ToUpperInvariant();
                if (uf is not null && !Ufs.Contains(uf))
                    throw new ValidacaoException("UF do CRM inválida.");
                break;
            default:
                throw new ValidacaoException("Tipo de médico solicitante inválido.");
        }

        var preco = d.Preco ?? throw new ValidacaoException("Informe o preço.");
        if (preco < 0)
            throw new ValidacaoException("O preço não pode ser negativo.");
        if (decimal.Round(preco, 2) != preco)
            throw new ValidacaoException("O preço deve ter no máximo 2 casas decimais.");
        GarantirPiso(preco - Desconto, catalogo.PrecoLimite);

        OrigemId = d.OrigemId;
        DestinoId = d.DestinoId;
        TipoMedico = d.TipoMedico;
        NomeMedicoExterno = nomeMedicoExterno;
        CrmMedico = crm;
        UfCrm = uf;
        Preco = preco;
    }
}

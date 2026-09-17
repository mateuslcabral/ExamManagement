using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Exame solicitado — registro de prontuário (Q45). Nasce aguardando amostra; o acolhimento e as etapas do laudo
/// fazem o estado avançar. Nunca é apagado fisicamente: a exclusão é lógica, com autor, data e motivo (C2, P4, P10).
/// </summary>
public class Exame
{
    /// <summary>Único médico interno (Q13); nome usado no modelo de mensagem de médico interno (D13).</summary>
    public const string NomeMedicoInterno = "Dr. Arsonval Lamounier Junior";
    public const int MaximoAnexos = 3; // Q22
    public const int TamanhoMaximoDestino = 200;
    public const int TamanhoMaximoNomeMedico = 200;
    public const int TamanhoMaximoMotivoExclusao = 500;

    public Guid Id { get; private set; }
    public Guid PacienteId { get; private set; }
    public Guid ExameCatalogoId { get; private set; }
    public OrigemExame Origem { get; private set; }

    /// <summary>Texto livre nesta versão (D7). Vira lista controlada quando o dashboard entrar (Q38.1).</summary>
    public string? Destino { get; private set; }

    public TipoMedico TipoMedico { get; private set; }

    /// <summary>Digitado a cada exame, sem cadastro (Q14, Q15). Nulo para médico interno.</summary>
    public string? NomeMedicoExterno { get; private set; }

    /// <summary>Herdado do preço de referência do catálogo e editável.</summary>
    public decimal Preco { get; private set; }

    public DateOnly DataEntrada { get; private set; }
    public EstadoExame Estado { get; private set; }

    /// <summary>
    /// Gravada uma única vez no acolhimento e nunca recalculada (P14): é a diferença para a data efetiva que torna
    /// o atraso mensurável. Nula enquanto não há amostra acolhida — inclusive após rejeição (Q19).
    /// </summary>
    public DateOnly? DataLiberacaoPrevista { get; private set; }

    private readonly List<Anexo> _anexos = [];
    public IReadOnlyCollection<Anexo> Anexos => _anexos.AsReadOnly();

    private readonly List<Amostra> _amostras = [];
    /// <summary>Histórico completo: amostras rejeitadas permanecem.</summary>
    public IReadOnlyCollection<Amostra> Amostras => _amostras.AsReadOnly();

    /// <summary>Amostra acolhida e não rejeitada. Há no máximo uma (P8).</summary>
    public Amostra? AmostraVigente => _amostras.SingleOrDefault(a => !a.Rejeitada);

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

    /// <param name="preco">Nulo usa o preço de referência do catálogo.</param>
    public static Exame Criar(
        Guid pacienteId, ExameCatalogo catalogo, OrigemExame origem, string? destino,
        TipoMedico tipoMedico, string? nomeMedicoExterno, decimal? preco, Guid autorId, DateOnly hoje)
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
        exame.Aplicar(origem, destino, tipoMedico, nomeMedicoExterno, preco ?? catalogo.PrecoReferencia);
        return exame;
    }

    public void Atualizar(
        ExameCatalogo catalogo, OrigemExame origem, string? destino,
        TipoMedico tipoMedico, string? nomeMedicoExterno, decimal preco, Guid autorId)
    {
        GarantirNaoExcluido();

        if (catalogo.Id != ExameCatalogoId)
        {
            // O prazo do catálogo alimenta a previsão gravada no acolhimento (P14): depois dele, trocar o exame
            // deixaria a data prevista incoerente com o exame registrado.
            if (Estado != EstadoExame.AguardandoAmostra)
                throw new ValidacaoException("O exame do catálogo só pode ser trocado antes do acolhimento da amostra.");
            if (!catalogo.Ativo)
                throw new ValidacaoException($"O exame '{catalogo.Nome}' está inativo no catálogo.");
        }

        Aplicar(origem, destino, tipoMedico, nomeMedicoExterno, preco);
        ExameCatalogoId = catalogo.Id;
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPorId = autorId;
    }

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

    /// <summary>
    /// Registra a chegada da amostra e fixa a previsão de liberação: acolhimento + prazo de execução + dias de revisão
    /// (Q1, P13). A data é digitável porque a amostra pode chegar num dia e ser lançada noutro (Q1.2 — a confirmar).
    /// </summary>
    /// <param name="catalogo">Item do catálogo deste exame, de onde vem o prazo de execução atual.</param>
    /// <param name="hoje">Data local da clínica.</param>
    public void AcolherAmostra(DateOnly dataAcolhimento, ExameCatalogo catalogo, int diasRevisao, Guid autorId, DateOnly hoje)
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
        if (diasRevisao < 0)
            throw new InvalidOperationException("Dias de revisão negativos.");

        var amostra = Amostra.Acolher(
            dataAcolhimento, catalogo.PrazoExecucaoDias, diasRevisao, recoleta: _amostras.Count > 0, autorId);
        _amostras.Add(amostra);
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
        DataLiberacaoPrevista = null;
        Estado = EstadoExame.AguardandoAmostra;
    }

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

    private void GarantirNaoExcluido()
    {
        if (Excluido) throw new ValidacaoException("Este exame foi excluído e não pode ser alterado.");
    }

    private void Aplicar(OrigemExame origem, string? destino, TipoMedico tipoMedico, string? nomeMedicoExterno, decimal preco)
    {
        if (!Enum.IsDefined(origem))
            throw new ValidacaoException("Origem inválida.");

        destino = string.IsNullOrWhiteSpace(destino) ? null : destino.Trim();
        if (destino?.Length > TamanhoMaximoDestino)
            throw new ValidacaoException($"O destino deve ter no máximo {TamanhoMaximoDestino} caracteres.");

        switch (tipoMedico)
        {
            case TipoMedico.Interno:
                nomeMedicoExterno = null;
                break;
            case TipoMedico.Externo:
                if (string.IsNullOrWhiteSpace(nomeMedicoExterno))
                    throw new ValidacaoException("Informe o nome do médico solicitante externo.");
                nomeMedicoExterno = nomeMedicoExterno.Trim();
                if (nomeMedicoExterno.Length > TamanhoMaximoNomeMedico)
                    throw new ValidacaoException($"O nome do médico deve ter no máximo {TamanhoMaximoNomeMedico} caracteres.");
                break;
            default:
                throw new ValidacaoException("Tipo de médico solicitante inválido.");
        }

        if (preco < 0)
            throw new ValidacaoException("O preço não pode ser negativo.");
        if (decimal.Round(preco, 2) != preco)
            throw new ValidacaoException("O preço deve ter no máximo 2 casas decimais.");

        Origem = origem;
        Destino = destino;
        TipoMedico = tipoMedico;
        NomeMedicoExterno = nomeMedicoExterno;
        Preco = preco;
    }
}

using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Exame solicitado (CLG-ESP §6). Agregado raiz: anexos (e, nas próximas etapas, amostra e etapas do laudo)
/// são acessados por aqui. Um paciente tem N exames; o vínculo é obrigatório.
/// </summary>
public class Exame
{
    public const string NomeMedicoInterno = "Dr. Arsonval Lamounier Junior";
    public const int MaximoAnexos = 3; // Q22

    public Guid Id { get; private set; }
    public Guid PacienteId { get; private set; }
    public Paciente Paciente { get; private set; } = null!;
    public Guid ExameCatalogoId { get; private set; }
    public ExameCatalogo ExameCatalogo { get; private set; } = null!;

    public OrigemExame Origem { get; private set; }
    public string? Destino { get; private set; }
    public TipoMedicoSolicitante TipoMedico { get; private set; }
    public string NomeMedico { get; private set; } = null!;

    public DateOnly DataEntrada { get; private set; }
    public decimal Preco { get; private set; }
    public EstadoExame Estado { get; private set; }

    /// <summary>Gravada uma única vez no acolhimento; não recalculada (P14).</summary>
    public DateOnly? DataLiberacaoPrevista { get; private set; }
    /// <summary>Gravada ao disponibilizar ao paciente (P14).</summary>
    public DateTime? DataLiberacaoEfetiva { get; private set; }

    private readonly List<Anexo> _anexos = [];
    public IReadOnlyCollection<Anexo> Anexos => _anexos.AsReadOnly();

    private readonly List<Amostra> _amostras = [];
    /// <summary>Histórico de acolhimentos (P30). Só uma amostra fica ativa.</summary>
    public IReadOnlyCollection<Amostra> Amostras => _amostras.AsReadOnly();
    public Amostra? AmostraAtiva => _amostras.FirstOrDefault(a => a.Ativa);

    private readonly List<EtapaAndamento> _etapas = [];
    public IReadOnlyCollection<EtapaAndamento> Etapas => _etapas.AsReadOnly();
    public EtapaAndamento? Etapa(TipoEtapaLaudo tipo) => _etapas.FirstOrDefault(e => e.Tipo == tipo);

    // Exclusão lógica (C2, P4, P10).
    public DateTime? ExcluidoEm { get; private set; }
    public Guid? ExcluidoPorUsuarioId { get; private set; }
    public string? MotivoExclusao { get; private set; }
    public bool Excluido => ExcluidoEm is not null;

    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    private Exame() { } // EF Core

    public static Exame Criar(
        Paciente paciente,
        ExameCatalogo catalogo,
        OrigemExame origem,
        string? destino,
        TipoMedicoSolicitante tipoMedico,
        string? nomeMedico,
        decimal? preco,
        DateOnly hoje)
    {
        if (paciente.Excluido)
            throw new PacienteExcluidoException();
        if (!catalogo.Ativo)
            throw new ValidacaoException("Este exame do catálogo está inativo.");

        var e = new Exame
        {
            Id = Guid.NewGuid(),
            PacienteId = paciente.Id,
            Paciente = paciente,
            ExameCatalogoId = catalogo.Id,
            ExameCatalogo = catalogo,
            DataEntrada = hoje,
            Estado = EstadoExame.AguardandoAmostra,
            CriadoEm = DateTime.UtcNow
        };
        // Preço herdado do catálogo e editável (P26).
        e.Aplicar(origem, destino, tipoMedico, nomeMedico, preco ?? catalogo.PrecoReferencia);
        return e;
    }

    public void Atualizar(OrigemExame origem, string? destino, TipoMedicoSolicitante tipoMedico, string? nomeMedico, decimal preco)
    {
        GarantirNaoExcluido();
        Aplicar(origem, destino, tipoMedico, nomeMedico, preco);
        AtualizadoEm = DateTime.UtcNow;
    }

    private void Aplicar(OrigemExame origem, string? destino, TipoMedicoSolicitante tipoMedico, string? nomeMedico, decimal preco)
    {
        if (!Enum.IsDefined(origem))
            throw new ValidacaoException("Origem inválida.");
        if (preco < 0)
            throw new ValidacaoException("O preço não pode ser negativo.");

        Origem = origem;
        Destino = string.IsNullOrWhiteSpace(destino) ? null : destino.Trim();
        TipoMedico = tipoMedico;
        NomeMedico = tipoMedico switch
        {
            TipoMedicoSolicitante.Interno => NomeMedicoInterno,
            TipoMedicoSolicitante.Externo when !string.IsNullOrWhiteSpace(nomeMedico) => nomeMedico.Trim(),
            TipoMedicoSolicitante.Externo => throw new ValidacaoException("Informe o nome do médico externo."),
            _ => throw new ValidacaoException("Tipo de médico inválido.")
        };
        Preco = decimal.Round(preco, 2);
    }

    public Anexo AdicionarAnexo(string nomeOriginal, string caminho, long tamanhoBytes, string hashSha256)
    {
        GarantirNaoExcluido();
        if (_anexos.Count >= MaximoAnexos)
            throw new LimiteDeAnexosException();

        var anexo = Anexo.Criar(Id, nomeOriginal, caminho, tamanhoBytes, hashSha256);
        _anexos.Add(anexo);
        AtualizadoEm = DateTime.UtcNow;
        return anexo;
    }

    public Anexo RemoverAnexo(Guid anexoId)
    {
        GarantirNaoExcluido();
        var anexo = _anexos.FirstOrDefault(a => a.Id == anexoId) ?? throw new RegistroNaoEncontradoException("Anexo");
        _anexos.Remove(anexo);
        AtualizadoEm = DateTime.UtcNow;
        return anexo;
    }

    // ---- Amostra e prazo (P29–P31) ----

    /// <summary>1→2: acolhe a amostra e grava a previsão uma única vez (P14, P30).</summary>
    public Amostra AcolherAmostra(DateOnly dataAcolhimento, Guid usuarioId, int diasRevisao, DateOnly hoje)
    {
        GarantirNaoExcluido();
        if (Estado != EstadoExame.AguardandoAmostra)
            throw new EstadoInvalidoException("acolher amostra", DescreverEstado());
        if (dataAcolhimento > hoje)
            throw new ValidacaoException("A data de acolhimento não pode ser futura.");
        if (dataAcolhimento < DataEntrada)
            throw new ValidacaoException("A data de acolhimento não pode ser anterior à entrada do exame.");
        if (diasRevisao < 0)
            throw new ValidacaoException("Dias de revisão inválidos.");

        var amostra = Amostra.Acolher(Id, dataAcolhimento, usuarioId);
        _amostras.Add(amostra);
        DataLiberacaoPrevista = dataAcolhimento.AddDays(ExameCatalogo.PrazoExecucaoDias + diasRevisao);
        Estado = EstadoExame.AmostraAcolhida;
        AtualizadoEm = DateTime.UtcNow;
        return amostra;
    }

    /// <summary>2→1: rejeição zera o prazo; a recoleta é um novo acolhimento (P31).</summary>
    public void RejeitarAmostra(string motivo, Guid usuarioId)
    {
        GarantirNaoExcluido();
        if (Estado != EstadoExame.AmostraAcolhida)
            throw new EstadoInvalidoException("rejeitar a amostra", DescreverEstado());
        var amostra = AmostraAtiva ?? throw new AmostraNaoAcolhidaException();

        amostra.Rejeitar(motivo, usuarioId);
        DataLiberacaoPrevista = null;
        Estado = EstadoExame.AguardandoAmostra;
        AtualizadoEm = DateTime.UtcNow;
    }

    // ---- Fluxo do laudo (P32–P34) ----

    /// <summary>
    /// Registra a etapa seguinte (2→3, 3→4, 4→5) com data automática, ou substitui o arquivo de uma etapa
    /// já registrada sem mudar estado. Devolve o caminho do arquivo anterior quando houve substituição.
    /// </summary>
    public (EtapaAndamento Etapa, string? CaminhoAnterior) RegistrarOuSubstituirEtapa(
        TipoEtapaLaudo tipo, string nomeOriginal, string caminho, long tamanhoBytes, string hash, Guid usuarioId)
    {
        GarantirNaoExcluido();

        var existente = Etapa(tipo);
        if (existente is not null)
            return (existente, existente.SubstituirArquivo(nomeOriginal, caminho, tamanhoBytes, hash));

        var estadoEsperado = (EstadoExame)((int)tipo - 1);
        if (Estado != estadoEsperado)
            throw new EstadoInvalidoException($"registrar '{DescreverEtapa(tipo)}'", DescreverEstado());

        var etapa = EtapaAndamento.Registrar(Id, tipo, nomeOriginal, caminho, tamanhoBytes, hash, usuarioId);
        _etapas.Add(etapa);
        Estado = (EstadoExame)(int)tipo;
        AtualizadoEm = DateTime.UtcNow;
        return (etapa, null);
    }

    /// <summary>5→6: ato manual (D10). Grava a data efetiva; quem chama dispara as notificações.</summary>
    public void Disponibilizar()
    {
        GarantirNaoExcluido();
        if (Estado != EstadoExame.LaudoRevisado)
            throw new EstadoInvalidoException("disponibilizar ao paciente", DescreverEstado());
        if (Etapa(TipoEtapaLaudo.LaudoRevisado) is null)
            throw new ValidacaoException("O laudo revisado não foi enviado.");

        DataLiberacaoEfetiva = DateTime.UtcNow;
        Estado = EstadoExame.DisponibilizadoAoPaciente;
        AtualizadoEm = DateTime.UtcNow;
    }

    public bool Disponibilizado => Estado == EstadoExame.DisponibilizadoAoPaciente;

    public static string DescreverEstado(EstadoExame estado) => estado switch
    {
        EstadoExame.AguardandoAmostra => "Cadastrado — aguardando amostra",
        EstadoExame.AmostraAcolhida => "Amostra acolhida",
        EstadoExame.LaudoParceiroPronto => "Laudo parceiro pronto",
        EstadoExame.LaudoCligenParaRevisao => "Laudo Cligen para revisão",
        EstadoExame.LaudoRevisado => "Laudo revisado",
        EstadoExame.DisponibilizadoAoPaciente => "Disponibilizado ao paciente",
        _ => estado.ToString()
    };

    public static string DescreverEtapa(TipoEtapaLaudo tipo) => DescreverEstado((EstadoExame)(int)tipo);

    private string DescreverEstado() => DescreverEstado(Estado);

    public void Excluir(Guid usuarioId, string motivo)
    {
        GarantirNaoExcluido();
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ValidacaoException("O motivo da exclusão é obrigatório.");
        if (usuarioId == Guid.Empty)
            throw new ValidacaoException("Autor da exclusão não identificado.");

        ExcluidoEm = DateTime.UtcNow;
        ExcluidoPorUsuarioId = usuarioId;
        MotivoExclusao = motivo.Trim();
    }

    public void Restaurar()
    {
        if (!Excluido)
            throw new ValidacaoException("Este exame não está excluído.");
        ExcluidoEm = null;
        ExcluidoPorUsuarioId = null;
        MotivoExclusao = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    private void GarantirNaoExcluido()
    {
        if (Excluido) throw new ExameExcluidoException();
    }
}

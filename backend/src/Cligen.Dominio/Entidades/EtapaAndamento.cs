using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Uma das três etapas do laudo (CLG-ESP §8, D9): data automática no primeiro upload e arquivo PDF
/// substituível (Q20) sem alterar a data da etapa (P32). Só PDF, sem transformação (P33).
/// </summary>
public class EtapaAndamento
{
    public const long TamanhoMaximoBytes = 50L * 1024 * 1024; // P1

    public Guid Id { get; private set; }
    public Guid ExameId { get; private set; }
    public TipoEtapaLaudo Tipo { get; private set; }

    /// <summary>Data da etapa — gravada no primeiro upload e nunca alterada (D9).</summary>
    public DateTime Data { get; private set; }

    public string NomeOriginal { get; private set; } = null!;
    public string Caminho { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }
    public string HashSha256 { get; private set; } = null!;
    public Guid RegistradoPorUsuarioId { get; private set; }

    /// <summary>Preenchido quando o arquivo foi substituído após o registro (Q20).</summary>
    public DateTime? ArquivoSubstituidoEm { get; private set; }
    public int Substituicoes { get; private set; }

    private EtapaAndamento() { } // EF Core

    internal static EtapaAndamento Registrar(Guid exameId, TipoEtapaLaudo tipo, string nomeOriginal, string caminho, long tamanhoBytes, string hash, Guid usuarioId)
    {
        ValidarArquivo(nomeOriginal, tamanhoBytes, hash);
        if (usuarioId == Guid.Empty)
            throw new ValidacaoException("Autor do registro não identificado.");

        return new EtapaAndamento
        {
            Id = Guid.NewGuid(),
            ExameId = exameId,
            Tipo = tipo,
            Data = DateTime.UtcNow,
            NomeOriginal = nomeOriginal.Trim(),
            Caminho = caminho,
            TamanhoBytes = tamanhoBytes,
            HashSha256 = hash,
            RegistradoPorUsuarioId = usuarioId
        };
    }

    /// <summary>Troca o arquivo (enviado por engano) e devolve o caminho antigo para remoção do storage.</summary>
    internal string SubstituirArquivo(string nomeOriginal, string caminho, long tamanhoBytes, string hash)
    {
        ValidarArquivo(nomeOriginal, tamanhoBytes, hash);
        var anterior = Caminho;
        NomeOriginal = nomeOriginal.Trim();
        Caminho = caminho;
        TamanhoBytes = tamanhoBytes;
        HashSha256 = hash;
        ArquivoSubstituidoEm = DateTime.UtcNow;
        Substituicoes++;
        return anterior;
    }

    public static void ValidarArquivo(string nomeOriginal, long tamanhoBytes, string? hash = "ok")
    {
        if (string.IsNullOrWhiteSpace(nomeOriginal) || !nomeOriginal.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ValidacaoException("O laudo deve ser um arquivo PDF.");
        if (tamanhoBytes <= 0)
            throw new ValidacaoException("O arquivo está vazio.");
        if (tamanhoBytes > TamanhoMaximoBytes)
            throw new ValidacaoException("O arquivo excede o limite de 50 MB.");
        if (string.IsNullOrWhiteSpace(hash))
            throw new ValidacaoException("Hash do arquivo ausente.");
    }
}

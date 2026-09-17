using Cligen.Dominio.Comum;
using Cligen.Dominio.Enums;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Uma das três etapas do laudo (D9), acessada sempre através de <see cref="Exame"/>. A data é gravada no primeiro
/// upload e nunca muda; o arquivo pode ser substituído (Q20) sem alterar data nem estado. Só PDF, gravado sem
/// transformação — laudo assinado fora da plataforma (Q44). O arquivo substituído não é apagado: fica sem referência.
/// </summary>
public class EtapaAndamento
{
    public Guid Id { get; private set; }
    public TipoEtapaLaudo Tipo { get; private set; }

    /// <summary>Data da etapa — do primeiro upload, nunca alterada (D9).</summary>
    public DateTime Data { get; private set; }

    public string NomeOriginal { get; private set; } = null!;
    public string Caminho { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }
    public string HashSha256 { get; private set; } = null!;
    public Guid RegistradoPorId { get; private set; }

    public int Substituicoes { get; private set; }
    public DateTime? SubstituidoEm { get; private set; }
    public Guid? SubstituidoPorId { get; private set; }

    private EtapaAndamento() { } // EF Core

    internal static EtapaAndamento Registrar(TipoEtapaLaudo tipo, ArquivoLaudo arquivo, Guid autorId)
    {
        var etapa = new EtapaAndamento
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Data = DateTime.UtcNow,
            RegistradoPorId = autorId
        };
        etapa.Aplicar(arquivo);
        return etapa;
    }

    internal void SubstituirArquivo(ArquivoLaudo arquivo, Guid autorId)
    {
        Aplicar(arquivo);
        Substituicoes++;
        SubstituidoEm = DateTime.UtcNow;
        SubstituidoPorId = autorId;
    }

    private void Aplicar(ArquivoLaudo arquivo)
    {
        if (string.IsNullOrWhiteSpace(arquivo.HashSha256) || string.IsNullOrWhiteSpace(arquivo.Caminho))
            throw new ValidacaoException("Arquivo não armazenado.");

        var nome = Path.GetFileName(arquivo.NomeOriginal?.Trim() ?? "");
        if (nome.Length == 0) nome = "laudo.pdf";
        if (nome.Length > Anexo.TamanhoMaximoNomeOriginal)
            nome = nome[..(Anexo.TamanhoMaximoNomeOriginal - 4)] + ".pdf";

        NomeOriginal = nome;
        Caminho = arquivo.Caminho;
        TamanhoBytes = arquivo.TamanhoBytes;
        HashSha256 = arquivo.HashSha256;
    }
}

/// <summary>Arquivo de laudo já gravado no armazenamento. O formato foi conferido antes: <see cref="FormatoArquivo.Pdf"/>.</summary>
public sealed record ArquivoLaudo(string? NomeOriginal, string Caminho, long TamanhoBytes, string HashSha256);

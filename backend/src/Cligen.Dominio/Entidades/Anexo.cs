using Cligen.Dominio.Comum;
using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Arquivo anexado ao exame, sem nomeação semântica (Q22) e nunca visível ao paciente (Q23). O binário fica fora
/// do banco; aqui ficam caminho, nome original, tipo, tamanho e hash. Remoção é lógica: o arquivo é preservado.
/// </summary>
public class Anexo
{
    public const int TamanhoMaximoNomeOriginal = 255;

    public Guid Id { get; private set; }
    public string NomeOriginal { get; private set; } = null!;
    public string TipoConteudo { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }

    /// <summary>SHA-256 em hexadecimal, calculado no armazenamento — base da verificação de integridade do acervo.</summary>
    public string HashSha256 { get; private set; } = null!;

    /// <summary>Chave do arquivo no armazenamento. Opaca para o domínio.</summary>
    public string Caminho { get; private set; } = null!;

    public DateTime EnviadoEm { get; private set; }
    public Guid EnviadoPorId { get; private set; }
    public DateTime? RemovidoEm { get; private set; }
    public Guid? RemovidoPorId { get; private set; }

    public bool Ativo => RemovidoEm is null;

    private Anexo() { } // EF Core

    public static Anexo Criar(
        string? nomeOriginal, FormatoArquivo formato, long tamanhoBytes, string hashSha256, string caminho, Guid autorId)
    {
        if (string.IsNullOrWhiteSpace(hashSha256) || string.IsNullOrWhiteSpace(caminho))
            throw new ValidacaoException("Arquivo não armazenado.");

        nomeOriginal = Path.GetFileName(nomeOriginal?.Trim() ?? "");
        if (nomeOriginal.Length == 0) nomeOriginal = "anexo" + formato.Extensao;
        if (nomeOriginal.Length > TamanhoMaximoNomeOriginal)
            nomeOriginal = nomeOriginal[..(TamanhoMaximoNomeOriginal - formato.Extensao.Length)] + formato.Extensao;

        return new Anexo
        {
            Id = Guid.NewGuid(),
            NomeOriginal = nomeOriginal,
            TipoConteudo = formato.TipoConteudo,
            TamanhoBytes = tamanhoBytes,
            HashSha256 = hashSha256,
            Caminho = caminho,
            EnviadoEm = DateTime.UtcNow,
            EnviadoPorId = autorId
        };
    }

    internal void Remover(Guid autorId)
    {
        if (!Ativo) throw new ValidacaoException("Este anexo já foi removido.");
        RemovidoEm = DateTime.UtcNow;
        RemovidoPorId = autorId;
    }
}

using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Arquivo vinculado ao exame (Q22). O binário fica fora do banco (P27); aqui só caminho, tipo, tamanho e hash.
/// Não visível ao paciente (Q23).
/// </summary>
public class Anexo
{
    public const int TamanhoMaximoBytes = 50 * 1024 * 1024; // P1
    public static readonly IReadOnlyDictionary<string, string> TiposPermitidos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
    };

    public Guid Id { get; private set; }
    public Guid ExameId { get; private set; }
    public string NomeOriginal { get; private set; } = null!;
    public string Caminho { get; private set; } = null!;
    public string TipoConteudo { get; private set; } = null!;
    public long TamanhoBytes { get; private set; }
    public string HashSha256 { get; private set; } = null!;
    public DateTime EnviadoEm { get; private set; }

    private Anexo() { } // EF Core

    internal static Anexo Criar(Guid exameId, string nomeOriginal, string caminho, long tamanhoBytes, string hashSha256)
    {
        if (string.IsNullOrWhiteSpace(nomeOriginal))
            throw new ValidacaoException("Nome do arquivo inválido.");
        var extensao = Path.GetExtension(nomeOriginal);
        if (!TiposPermitidos.TryGetValue(extensao, out var tipo))
            throw new ValidacaoException("Formato não permitido. Envie PDF, JPG ou PNG.");
        if (tamanhoBytes <= 0)
            throw new ValidacaoException("O arquivo está vazio.");
        if (tamanhoBytes > TamanhoMaximoBytes)
            throw new ValidacaoException("O arquivo excede o limite de 50 MB.");
        if (string.IsNullOrWhiteSpace(hashSha256))
            throw new ValidacaoException("Hash do arquivo ausente.");

        return new Anexo
        {
            Id = Guid.NewGuid(),
            ExameId = exameId,
            NomeOriginal = nomeOriginal.Trim(),
            Caminho = caminho,
            TipoConteudo = tipo,
            TamanhoBytes = tamanhoBytes,
            HashSha256 = hashSha256,
            EnviadoEm = DateTime.UtcNow
        };
    }
}

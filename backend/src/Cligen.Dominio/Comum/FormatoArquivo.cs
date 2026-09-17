using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Comum;

/// <summary>
/// Formatos aceitos para anexos e laudos: PDF, JPG e PNG (P1). O tipo é decidido pelos primeiros bytes do
/// arquivo, não pelo nome nem pelo Content-Type informado — ambos são controlados por quem envia.
/// </summary>
public sealed record FormatoArquivo(string Extensao, string TipoConteudo)
{
    public const long TamanhoMaximoBytes = 50L * 1024 * 1024; // P1
    public const int BytesCabecalho = 8;

    public static readonly FormatoArquivo Pdf = new(".pdf", "application/pdf");
    public static readonly FormatoArquivo Jpg = new(".jpg", "image/jpeg");
    public static readonly FormatoArquivo Png = new(".png", "image/png");

    private static readonly byte[] AssinaturaPdf = "%PDF-"u8.ToArray();
    private static readonly byte[] AssinaturaPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] AssinaturaJpg = [0xFF, 0xD8, 0xFF];

    public static FormatoArquivo Detectar(ReadOnlySpan<byte> cabecalho, long tamanhoBytes)
    {
        if (tamanhoBytes <= 0)
            throw new ValidacaoException("O arquivo está vazio.");
        if (tamanhoBytes > TamanhoMaximoBytes)
            throw new ValidacaoException("O arquivo excede o limite de 50 MB.");

        if (cabecalho.StartsWith(AssinaturaPdf)) return Pdf;
        if (cabecalho.StartsWith(AssinaturaPng)) return Png;
        if (cabecalho.StartsWith(AssinaturaJpg)) return Jpg;
        throw new ValidacaoException("Formato não aceito: envie PDF, JPG ou PNG.");
    }
}

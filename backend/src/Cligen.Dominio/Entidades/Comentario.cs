using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Anotação da equipe num exame em andamento, acessada através de <see cref="Exame"/>. Só inserção: comentário
/// não se edita nem se apaga (prontuário).
/// </summary>
public class Comentario
{
    public const int TamanhoMaximoTexto = 2000;

    public Guid Id { get; private set; }
    public string Texto { get; private set; } = null!;
    public Guid AutorId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Comentario() { } // EF Core

    internal static Comentario Criar(string? texto, Guid autorId)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ValidacaoException("Escreva o comentário.");
        texto = texto.Trim();
        if (texto.Length > TamanhoMaximoTexto)
            throw new ValidacaoException($"O comentário deve ter no máximo {TamanhoMaximoTexto} caracteres.");

        return new Comentario { Id = Guid.NewGuid(), Texto = texto, AutorId = autorId, CriadoEm = DateTime.UtcNow };
    }
}

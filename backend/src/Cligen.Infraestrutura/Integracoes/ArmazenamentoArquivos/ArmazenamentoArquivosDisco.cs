using Cligen.Aplicacao.Interfaces.Servicos;
using Microsoft.Extensions.Options;

namespace Cligen.Infraestrutura.Integracoes.ArmazenamentoArquivos;

public sealed class OpcoesArmazenamentoDisco
{
    public const string Secao = "Armazenamento";

    /// <summary>Diretório base dos binários. Relativo ao diretório de trabalho da API se não for absoluto.</summary>
    public string DiretorioBase { get; set; } = "dados/arquivos";
}

/// <summary>
/// PROVISÓRIO (P27): grava em disco local do servidor da API. Sem redundância nem backup — não serve
/// para produção. Substituir por object storage (Azure Blob/S3) quando a hospedagem for definida;
/// o caminho relativo gravado em <c>Anexo.Caminho</c> vira a chave do objeto na migração.
/// </summary>
public sealed class ArmazenamentoArquivosDisco(IOptions<OpcoesArmazenamentoDisco> opcoes) : IArmazenamentoArquivos
{
    private readonly string _base = Path.GetFullPath(opcoes.Value.DiretorioBase);

    public async Task<string> SalvarAsync(string caminhoRelativo, Stream conteudo, CancellationToken ct = default)
    {
        var destino = Resolver(caminhoRelativo);
        Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
        await using var arquivo = new FileStream(destino, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await conteudo.CopyToAsync(arquivo, ct);
        return caminhoRelativo.Replace('\\', '/');
    }

    public Task<Stream> AbrirAsync(string caminho, CancellationToken ct = default)
    {
        var origem = Resolver(caminho);
        if (!File.Exists(origem))
            throw new FileNotFoundException("Arquivo do anexo não encontrado no armazenamento.", origem);
        Stream s = new FileStream(origem, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult(s);
    }

    public Task RemoverAsync(string caminho, CancellationToken ct = default)
    {
        var alvo = Resolver(caminho);
        if (File.Exists(alvo)) File.Delete(alvo);
        return Task.CompletedTask;
    }

    /// <summary>Impede path traversal: o caminho resolvido precisa ficar dentro do diretório base.</summary>
    private string Resolver(string caminhoRelativo)
    {
        var completo = Path.GetFullPath(Path.Combine(_base, caminhoRelativo));
        if (!completo.StartsWith(_base + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Caminho de arquivo inválido.");
        return completo;
    }
}

using System.Security.Cryptography;
using Cligen.Aplicacao.Interfaces.Servicos;
using Microsoft.Extensions.Options;

namespace Cligen.Infraestrutura.Integracoes.ArmazenamentoArquivos;

public sealed class OpcoesArmazenamentoLocal
{
    public const string Secao = "ArmazenamentoLocal";

    /// <summary>Diretório raiz. Relativo é resolvido a partir do content root da Api.</summary>
    public string Diretorio { get; set; } = "App_Data/arquivos";
}

/// <summary>
/// Implementação provisória em disco local, até a escolha do object storage (pendência de hospedagem).
/// Organiza por ano/mês de envio, grava os bytes sem alteração e nunca apaga. Sem redundância: não usar em produção
/// sem backup do diretório.
/// </summary>
public sealed class ArmazenamentoArquivosLocal(IOptions<OpcoesArmazenamentoLocal> opcoes) : IArmazenamentoArquivos
{
    private readonly string _raiz = Path.GetFullPath(opcoes.Value.Diretorio);

    public async Task<ArquivoArmazenado> SalvarAsync(Stream conteudo, string extensao, CancellationToken ct = default)
    {
        var agora = DateTime.UtcNow;
        var caminho = $"{agora:yyyy}/{agora:MM}/{Guid.NewGuid():N}{extensao}";
        var destino = CaminhoAbsoluto(caminho);
        Directory.CreateDirectory(Path.GetDirectoryName(destino)!);

        // Grava em temporário e só então move: um upload interrompido nunca deixa arquivo parcial no caminho final.
        var temporario = destino + ".parcial";
        string hash;
        long tamanho;
        try
        {
            await using (var arquivo = new FileStream(temporario, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            using (var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                var buffer = new byte[81920];
                int lidos;
                while ((lidos = await conteudo.ReadAsync(buffer, ct)) > 0)
                {
                    sha.AppendData(buffer, 0, lidos);
                    await arquivo.WriteAsync(buffer.AsMemory(0, lidos), ct);
                }
                await arquivo.FlushAsync(ct);
                hash = Convert.ToHexStringLower(sha.GetHashAndReset());
                tamanho = arquivo.Length;
            }
            File.Move(temporario, destino);
        }
        catch
        {
            File.Delete(temporario); // só o temporário: nada que tenha chegado ao caminho final é apagado
            throw;
        }

        return new ArquivoArmazenado(caminho, tamanho, hash);
    }

    public Task<Stream> AbrirLeituraAsync(string caminho, CancellationToken ct = default)
    {
        var absoluto = CaminhoAbsoluto(caminho);
        if (!File.Exists(absoluto))
            throw new FileNotFoundException("Arquivo não encontrado no armazenamento.", caminho);
        Stream stream = new FileStream(absoluto, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    private string CaminhoAbsoluto(string caminho)
    {
        var absoluto = Path.GetFullPath(Path.Combine(_raiz, caminho));
        // O caminho vem do banco, mas nunca deve escapar da raiz (defesa contra dado adulterado).
        if (!absoluto.StartsWith(_raiz + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Caminho de arquivo fora do armazenamento.");
        return absoluto;
    }
}

using System.Security.Cryptography;
using System.Text;
using Cligen.Aplicacao.Interfaces.Servicos;
using Microsoft.Extensions.Options;

namespace Cligen.Infraestrutura.Seguranca;

public sealed class OpcoesTokenDefinicaoSenha
{
    public const string Secao = "TokenDefinicaoSenha";

    /// <summary>Segredo HMAC. Em produção, vir de variável de ambiente / cofre, nunca do appsettings.</summary>
    public string Segredo { get; set; } = "";

    /// <summary>Validade do link — pendência em aberto (docs/06-pendencias). Padrão 24 h.</summary>
    public int ValidadeHoras { get; set; } = 24;
}

/// <summary>
/// Token auto-contido assinado por HMAC-SHA256: {usuarioId}.{expiraUnix}.{assinatura}.
/// Sem estado no banco — simples, suficiente para 4 usuários; não é revogável individualmente
/// antes de expirar (aceitável nesta fase; reavaliar junto com a política de senha).
/// </summary>
public sealed class TokenDefinicaoSenhaHmac(IOptions<OpcoesTokenDefinicaoSenha> opcoes) : ITokenDefinicaoSenha
{
    private readonly OpcoesTokenDefinicaoSenha _o = opcoes.Value;

    public string Gerar(Guid usuarioId)
    {
        var expira = DateTimeOffset.UtcNow.AddHours(_o.ValidadeHoras).ToUnixTimeSeconds();
        var payload = $"{usuarioId:N}.{expira}";
        return $"{payload}.{Assinar(payload)}";
    }

    public Guid? Validar(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var partes = token.Split('.');
        if (partes.Length != 3) return null;

        var payload = $"{partes[0]}.{partes[1]}";
        var esperado = Assinar(payload);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(esperado), Encoding.ASCII.GetBytes(partes[2])))
            return null;

        if (!long.TryParse(partes[1], out var expira)) return null;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expira) return null;

        return Guid.TryParseExact(partes[0], "N", out var id) ? id : null;
    }

    private string Assinar(string payload)
    {
        if (string.IsNullOrEmpty(_o.Segredo))
            throw new InvalidOperationException("TokenDefinicaoSenha:Segredo não configurado.");
        var mac = HMACSHA256.HashData(Encoding.UTF8.GetBytes(_o.Segredo), Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(mac).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}

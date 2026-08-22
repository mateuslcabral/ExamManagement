using Cligen.Aplicacao.Interfaces.Servicos;

namespace Cligen.Api.Configuracao;

/// <summary>Aponta para a página do frontend (sistema interno) onde o usuário define a senha.</summary>
public sealed class UrlDefinicaoSenha(IConfiguration config) : IUrlDefinicaoSenha
{
    private readonly string _base = (config["Frontend:SistemaUrl"]
        ?? throw new InvalidOperationException("Frontend:SistemaUrl não configurada.")).TrimEnd('/');

    public string Montar(string token) => $"{_base}/definir-senha?token={Uri.EscapeDataString(token)}";
}

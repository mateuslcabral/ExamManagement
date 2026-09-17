using Cligen.Aplicacao.Interfaces.Servicos;

namespace Cligen.Api.Configuracao;

/// <summary>Endereço do portal do paciente citado nas mensagens de boas-vindas.</summary>
public sealed class UrlPortalPaciente(IConfiguration config) : IUrlPortalPaciente
{
    private readonly string _url = (config["Frontend:PortalUrl"]
        ?? throw new InvalidOperationException("Frontend:PortalUrl não configurada.")).TrimEnd('/');

    public string Obter() => _url;
}

using Cligen.Aplicacao.DTOs;
using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;

namespace Cligen.Aplicacao.Servicos;

/// <summary>
/// Chamado exclusivamente pelo BFF (Next.js) do sistema interno. Valida e-mail/senha
/// e devolve a identidade; nunca emite sessão. Sessão é responsabilidade do BFF.
/// </summary>
public sealed class AutenticacaoService(IUsuarioRepositorio repositorio, IHashSenha hashSenha)
{
    /// <returns>Identidade, ou null se as credenciais não conferem (resposta deliberadamente opaca).</returns>
    public async Task<IdentidadeDto?> ValidarCredenciaisAsync(ValidarCredenciaisRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha))
            return null;

        var usuario = await repositorio.ObterPorEmailAsync(req.Email.Trim().ToLowerInvariant(), ct);
        if (usuario is null || !usuario.PodeAutenticarComSenha())
            return null;

        if (!hashSenha.Verificar(req.Senha, usuario.SenhaHash!))
            return null;

        return new IdentidadeDto(usuario.Id, usuario.Nome, usuario.Email, usuario.TipoLogin);
    }
}

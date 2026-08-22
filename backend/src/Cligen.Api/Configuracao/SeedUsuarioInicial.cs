using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Dominio.Entidades;
using Cligen.Dominio.Enums;

namespace Cligen.Api.Configuracao;

/// <summary>
/// Cria o primeiro usuário (quem vai cadastrar os demais pela Gestão de Usuários).
/// Sem ele, ninguém conseguiria entrar — é a única exceção à regra "conta nasce pela Gestão de Usuários".
/// Idempotente: não faz nada se o e-mail já existir. Senha inicial vem da configuração e deve ser trocada.
/// </summary>
public static class SeedUsuarioInicial
{
    public static async Task ExecutarAsync(IServiceProvider sp, IConfiguration config, ILogger logger)
    {
        var email = config["SeedAdmin:Email"];
        var nome = config["SeedAdmin:Nome"] ?? "Administrador";
        var senha = config["SeedAdmin:Senha"];

        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogInformation("SeedAdmin:Email não configurado — seed ignorado.");
            return;
        }

        using var scope = sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUsuarioRepositorio>();
        var hash = scope.ServiceProvider.GetRequiredService<IHashSenha>();

        email = Usuario.NormalizarEmail(email);
        if (await repo.ExisteEmailAsync(email)) return;

        var usuario = Usuario.Criar(nome, email);
        if (usuario.TipoLogin == TipoLogin.Local)
        {
            if (string.IsNullOrWhiteSpace(senha))
                throw new InvalidOperationException("SeedAdmin:Senha é obrigatória para e-mail de conta local.");
            usuario.DefinirSenha(hash.Gerar(senha));
        }

        await repo.AdicionarAsync(usuario);
        logger.LogWarning("Usuário inicial criado: {Email} ({Tipo}). Troque a senha após o primeiro acesso.", email, usuario.TipoLogin);
    }
}

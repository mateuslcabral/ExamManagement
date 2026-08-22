using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Infraestrutura.Integracoes.Email;
using Cligen.Infraestrutura.Persistencia;
using Cligen.Infraestrutura.Persistencia.Repositorios;
using Cligen.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cligen.Infraestrutura;

/// <summary>
/// Único ponto onde as interfaces da Aplicação são ligadas às implementações concretas.
/// A Api chama isto no Program.cs (composition root) e nunca referencia as classes daqui diretamente.
/// </summary>
public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarInfraestrutura(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<CligenDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("Cligen"),
                sql => sql.MigrationsAssembly(typeof(CligenDbContext).Assembly.FullName)));

        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

        services.AddSingleton<IHashSenha, HashSenhaIdentity>();
        services.Configure<OpcoesTokenDefinicaoSenha>(config.GetSection(OpcoesTokenDefinicaoSenha.Secao));
        services.AddSingleton<ITokenDefinicaoSenha, TokenDefinicaoSenhaHmac>();

        // Provisório até definição do provedor (Q37).
        services.AddSingleton<IEnvioEmail, EnvioEmailLog>();

        return services;
    }
}

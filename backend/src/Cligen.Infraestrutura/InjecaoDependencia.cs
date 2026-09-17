using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Infraestrutura.Integracoes.ArmazenamentoArquivos;
using Cligen.Infraestrutura.Integracoes.Email;
using Cligen.Infraestrutura.Integracoes.WhatsApp;
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
    /// <param name="contentRoot">Base para caminhos relativos da configuração (ex.: diretório do armazenamento local).</param>
    public static IServiceCollection AdicionarInfraestrutura(this IServiceCollection services, IConfiguration config, string contentRoot)
    {
        services.AddDbContext<CligenDbContext>(o =>
            o.UseSqlServer(config.GetConnectionString("Cligen"),
                sql => sql.MigrationsAssembly(typeof(CligenDbContext).Assembly.FullName)));

        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<IExameCatalogoRepositorio, ExameCatalogoRepositorio>();
        services.AddScoped<IParametroRepositorio, ParametroRepositorio>();
        services.AddScoped<IPacienteRepositorio, PacienteRepositorio>();
        services.AddScoped<IExameRepositorio, ExameRepositorio>();

        services.AddSingleton<IHashSenha, HashSenhaIdentity>();
        services.Configure<OpcoesTokenDefinicaoSenha>(config.GetSection(OpcoesTokenDefinicaoSenha.Secao));
        services.AddSingleton<ITokenDefinicaoSenha, TokenDefinicaoSenhaHmac>();

        // Provisório até definição do provedor (Q37).
        services.AddSingleton<IEnvioEmail, EnvioEmailLog>();
        // Provisório até integração com a WhatsApp Business API (C7, templates Meta).
        services.AddSingleton<IEnvioWhatsApp, EnvioWhatsAppLog>();

        // Provisório até a escolha do object storage (depende da hospedagem).
        services.Configure<OpcoesArmazenamentoLocal>(config.GetSection(OpcoesArmazenamentoLocal.Secao));
        services.PostConfigure<OpcoesArmazenamentoLocal>(o => o.Diretorio = Path.GetFullPath(o.Diretorio, contentRoot));
        services.AddSingleton<IArmazenamentoArquivos, ArmazenamentoArquivosLocal>();

        return services;
    }
}

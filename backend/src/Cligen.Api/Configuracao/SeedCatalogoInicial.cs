using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;

namespace Cligen.Api.Configuracao;

/// <summary>
/// Exames de exemplo para uso imediato (Q3) enquanto a lista oficial não é fornecida. Prazos e preços são
/// ilustrativos. Idempotente: só roda com o catálogo vazio. Desligue com <c>SeedCatalogo:Exemplos = false</c>.
/// </summary>
public static class SeedCatalogoInicial
{
    private static readonly DadosCatalogo[] Exemplos =
    [
        new("Sequenciamento Completo do Exoma", 48, 6500m, 5500m, 20000, "Todos os genes codificantes", null, "NGS, cobertura média 100×"),
        new("Painel de Cardiopatias Hereditárias", 33, 3200m, 2800m, 174, "MYBPC3, MYH7, TNNT2, TTN, LMNA, entre outros", null, "NGS, painel direcionado"),
        new("Cariótipo com Banda G", 24, 650m, 500m, null, null, null, "Citogenética clássica, 20 metáfases"),
    ];

    public static async Task ExecutarAsync(IServiceProvider sp, IConfiguration config, ILogger logger)
    {
        if (!config.GetValue("SeedCatalogo:Exemplos", true)) return;

        using var scope = sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IExameCatalogoRepositorio>();
        if ((await repo.ListarAsync()).Count > 0) return;

        foreach (var dados in Exemplos)
            await repo.AdicionarAsync(ExameCatalogo.Criar(dados, null));

        logger.LogInformation("Catálogo semeado com {Quantidade} exames de exemplo — substituir pela lista oficial.", Exemplos.Length);
    }
}

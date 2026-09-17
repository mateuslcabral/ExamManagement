using Cligen.Aplicacao.Interfaces.Repositorios;
using Cligen.Dominio.Entidades;

namespace Cligen.Api.Configuracao;

/// <summary>
/// Exames de exemplo para uso imediato (Q3) enquanto a lista oficial não é fornecida. Prazos e preços são
/// ilustrativos. Idempotente: só roda com o catálogo vazio. Desligue com <c>SeedCatalogo:Exemplos = false</c>.
/// </summary>
public static class SeedCatalogoInicial
{
    private static readonly (string Nome, int PrazoDias, decimal Preco)[] Exemplos =
    [
        ("Sequenciamento Completo do Exoma", 45, 6500m),
        ("Painel de Cardiopatias Hereditárias", 30, 3200m),
        ("Cariótipo com Banda G", 21, 650m),
    ];

    public static async Task ExecutarAsync(IServiceProvider sp, IConfiguration config, ILogger logger)
    {
        if (!config.GetValue("SeedCatalogo:Exemplos", true)) return;

        using var scope = sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IExameCatalogoRepositorio>();
        if ((await repo.ListarAsync()).Count > 0) return;

        foreach (var (nome, prazo, preco) in Exemplos)
            await repo.AdicionarAsync(ExameCatalogo.Criar(nome, prazo, preco));

        logger.LogInformation("Catálogo semeado com {Quantidade} exames de exemplo — substituir pela lista oficial.", Exemplos.Length);
    }
}

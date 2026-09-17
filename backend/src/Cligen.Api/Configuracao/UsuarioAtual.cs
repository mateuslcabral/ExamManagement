using Cligen.Aplicacao.Interfaces.Servicos;

namespace Cligen.Api.Configuracao;

/// <summary>Scoped por requisição; preenchido por <see cref="Middlewares.UsuarioAtualMiddleware"/>.</summary>
public sealed class UsuarioAtual : IUsuarioAtual
{
    private Guid? _id;

    public Guid Id => _id ?? throw new InvalidOperationException("Requisição sem usuário autenticado.");

    internal void Definir(Guid id) => _id = id;
}

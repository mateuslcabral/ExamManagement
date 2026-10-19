namespace Cligen.Aplicacao.DTOs;

/// <summary>Origem, destino ou convênio — mesma forma para os três.</summary>
public sealed record CadastroDto(Guid Id, string Nome, bool Ativo, DateTime CriadoEm, DateTime? AtualizadoEm);

public sealed record SalvarCadastroRequest(string Nome);

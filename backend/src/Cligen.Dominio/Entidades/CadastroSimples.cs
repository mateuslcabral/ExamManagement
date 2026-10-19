using Cligen.Dominio.Excecoes;

namespace Cligen.Dominio.Entidades;

/// <summary>
/// Cadastro auxiliar de nome único (origem, destino, convênio): pedido do cliente em 17/10/2026 para que esses
/// campos sejam listas suspensas. Nunca é apagado — item aposentado é desativado, preservando o histórico.
/// </summary>
public abstract class CadastroSimples
{
    public const int TamanhoMaximoNome = 150;

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }
    /// <summary>Nulo só nos itens semeados pela migration (origens iniciais), criados antes de existir usuário.</summary>
    public Guid? CriadoPorId { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }
    public Guid? AtualizadoPorId { get; private set; }

    protected CadastroSimples() { } // EF Core

    protected void Iniciar(string? nome, Guid autorId)
    {
        Id = Guid.NewGuid();
        Nome = NormalizarNome(nome);
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
        CriadoPorId = autorId;
    }

    public void Renomear(string? nome, Guid autorId) { Nome = NormalizarNome(nome); Tocar(autorId); }
    public void Desativar(Guid autorId) { Ativo = false; Tocar(autorId); }
    public void Reativar(Guid autorId) { Ativo = true; Tocar(autorId); }

    public static string NormalizarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidacaoException("O nome é obrigatório.");
        nome = nome.Trim();
        if (nome.Length > TamanhoMaximoNome)
            throw new ValidacaoException($"O nome deve ter no máximo {TamanhoMaximoNome} caracteres.");
        return nome;
    }

    private void Tocar(Guid autorId)
    {
        AtualizadoEm = DateTime.UtcNow;
        AtualizadoPorId = autorId;
    }
}

/// <summary>Permite ao serviço genérico criar instâncias do cadastro concreto.</summary>
public interface ICadastroCriavel<TSelf> where TSelf : CadastroSimples
{
    static abstract TSelf Criar(string? nome, Guid autorId);
    /// <summary>Nome legível, singular, para mensagens ("origem", "destino", "convênio").</summary>
    static abstract string Rotulo { get; }
}

/// <summary>Canal de entrada do exame (D4). Substitui a lista fixa Cligen / Clínica parceira / Site / Plataforma.</summary>
public sealed class Origem : CadastroSimples, ICadastroCriavel<Origem>
{
    private Origem() { }
    public static Origem Criar(string? nome, Guid autorId) { var o = new Origem(); o.Iniciar(nome, autorId); return o; }
    public static string Rotulo => "origem";
}

/// <summary>Laboratório/unidade que executa o exame (D7). Substitui o texto livre (Q38.1).</summary>
public sealed class Destino : CadastroSimples, ICadastroCriavel<Destino>
{
    private Destino() { }
    public static Destino Criar(string? nome, Guid autorId) { var d = new Destino(); d.Iniciar(nome, autorId); return d; }
    public static string Rotulo => "destino";
}

/// <summary>Operadora de convênio (C4.5 respondido em 17/10/2026: cadastro).</summary>
public sealed class Convenio : CadastroSimples, ICadastroCriavel<Convenio>
{
    private Convenio() { }
    public static Convenio Criar(string? nome, Guid autorId) { var c = new Convenio(); c.Iniciar(nome, autorId); return c; }
    public static string Rotulo => "convênio";
}

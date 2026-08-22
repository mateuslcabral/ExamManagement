using Cligen.Aplicacao.Interfaces.Servicos;
using Microsoft.AspNetCore.Identity;

namespace Cligen.Infraestrutura.Seguranca;

/// <summary>Delegação ao PasswordHasher do ASP.NET Core Identity (PBKDF2, versão 3).</summary>
public sealed class HashSenhaIdentity : IHashSenha
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object Dummy = new();

    public string Gerar(string senha) => _hasher.HashPassword(Dummy, senha);

    public bool Verificar(string senha, string hash)
        => _hasher.VerifyHashedPassword(Dummy, hash, senha) is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
}

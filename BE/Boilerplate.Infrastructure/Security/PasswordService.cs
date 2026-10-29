using Boilerplate.Application.InfraInterfaces;
using Boilerplate.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Boilerplate.Infrastructure.Security;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public string HashPassword(Account account, string password) =>
        _passwordHasher.HashPassword(account, password);

    public bool VerifyPassword(
        Account account,
        string passwordHash,
        string password) =>
        _passwordHasher.VerifyHashedPassword(account, passwordHash, password)
            is not PasswordVerificationResult.Failed;
}

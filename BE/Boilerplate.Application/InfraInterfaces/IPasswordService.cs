using Boilerplate.Domain.Entities;

namespace Boilerplate.Application.InfraInterfaces;

public interface IPasswordService
{
    string HashPassword(Account account, string password);

    bool VerifyPassword(Account account, string passwordHash, string password);
}

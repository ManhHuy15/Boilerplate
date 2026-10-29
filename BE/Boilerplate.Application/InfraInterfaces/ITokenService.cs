using Boilerplate.Domain.Entities;

namespace Boilerplate.Application.InfraInterfaces;

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(Account account);

    string CreateRefreshToken();

    string HashRefreshToken(string refreshToken);
}

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);

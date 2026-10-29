using Boilerplate.Application.InfraInterfaces;
using Boilerplate.Application.Services.RefreshTokenServices.DTOs;
using Boilerplate.Application.Settings;
using Boilerplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Boilerplate.Application.Services.RefreshTokenServices;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> CreateAsync(
        Account account,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenResult> RotateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenService(
        IApplicationDbContext dbContext,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<RefreshTokenResult> CreateAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        RefreshTokenResult result = Create(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<RefreshTokenResult> RotateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        string tokenHash = _tokenService.HashRefreshToken(refreshToken);
        RefreshToken? storedToken = await _dbContext.RefreshTokens
            .Include(token => token.Account)
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        DateTime utcNow = DateTime.UtcNow;
        if (storedToken is null ||
            !storedToken.IsActive(utcNow) ||
            !storedToken.Account.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
        }

        RefreshTokenResult result = Create(storedToken.Account);
        storedToken.RevokedAt = utcNow;
        storedToken.ReplacedByTokenHash = _tokenService.HashRefreshToken(result.Token);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task RevokeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        string tokenHash = _tokenService.HashRefreshToken(refreshToken);
        RefreshToken? storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private RefreshTokenResult Create(Account account)
    {
        string refreshToken = _tokenService.CreateRefreshToken();
        DateTime expiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            ExpiresAt = expiresAtUtc,
            Account = account
        });

        return new RefreshTokenResult(refreshToken, expiresAtUtc, account);
    }
}

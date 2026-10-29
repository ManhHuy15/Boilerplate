using Boilerplate.Application.InfraInterfaces;
using Boilerplate.Application.Services.AuthServices.DTOs;
using Boilerplate.Application.Services.RefreshTokenServices;
using Boilerplate.Application.Services.RefreshTokenServices.DTOs;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Boilerplate.Application.Services.AuthServices;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<AccountResponse> GetAccountAsync(
        string username,
        CancellationToken cancellationToken = default);
}

public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IApplicationDbContext dbContext,
        IPasswordService passwordService,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        string username = TrimUsername(request.Username);

        bool usernameExists = await _dbContext.Accounts
            .AnyAsync(
                account => account.Username == username,
                cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException("Username is already in use.");
        }

        var account = new Account
        {
            Username = username,
            Role = AccountRole.User,
            IsActive = true
        };

        account.PasswordHash = _passwordService.HashPassword(account, request.Password);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await CreateAndSaveSessionAsync(account, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        string username = TrimUsername(request.Username);
        Account? account = await _dbContext.Accounts
            .SingleOrDefaultAsync(
                candidate => candidate.Username == username,
                cancellationToken);

        if (account is null ||
            !account.IsActive ||
            !_passwordService.VerifyPassword(account, account.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        return await CreateAndSaveSessionAsync(account, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        RefreshTokenResult result = await _refreshTokenService.RotateAsync(
            refreshToken,
            cancellationToken);

        return CreateAuthResponse(result);
    }

    public async Task RevokeRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _refreshTokenService.RevokeAsync(refreshToken, cancellationToken);
    }

    public async Task<AccountResponse> GetAccountAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        AccountResponse? account = await _dbContext.Accounts
            .AsNoTracking()
            .Where(candidate => candidate.Username == username && candidate.IsActive)
            .Select(candidate => new AccountResponse(
                candidate.Id,
                candidate.Username,
                candidate.Role))
            .SingleOrDefaultAsync(cancellationToken);

        return account ?? throw new KeyNotFoundException("Account was not found.");
    }

    private AuthResponse CreateAuthResponse(RefreshTokenResult refreshToken)
    {
        AccessTokenResult accessToken = _tokenService.CreateAccessToken(refreshToken.Account);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            Account = new AccountResponse(
                refreshToken.Account.Id,
                refreshToken.Account.Username,
                refreshToken.Account.Role)
        };
    }

    private async Task<AuthResponse> CreateAndSaveSessionAsync(
        Account account,
        CancellationToken cancellationToken)
    {
        RefreshTokenResult refreshToken = await _refreshTokenService.CreateAsync(
            account,
            cancellationToken);

        return CreateAuthResponse(refreshToken);
    }

    private static string TrimUsername(string username) =>
        username.Trim();
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Boilerplate.Application.Services.AuthServices;
using Boilerplate.Application.Services.AuthServices.DTOs;
using Boilerplate.Application.Services.RefreshTokenServices;
using Boilerplate.Application.Settings;
using Boilerplate.Domain.Entities;
using Boilerplate.Domain.Enums;
using Boilerplate.Infrastructure.Data;
using Boilerplate.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Boilerplate.Tests.Auth;

public sealed class AuthWorkflowTests
{
    private static readonly JwtSettings DefaultJwtSettings = new()
    {
        SecretKey = "high-trust-verification-secret-key-with-at-least-32-bytes!",
        Issuer = "Boilerplate.API",
        Audience = "Boilerplate.Client",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 7,
        RefreshCookieName = "boilerplate_refresh_token",
        RefreshCookieSecure = true,
        RefreshCookieSameSite = "Strict"
    };

    [Fact]
    public async Task RegisterAsync_Trims_Username_Preserves_Casing_Assigns_User_Role_And_Stores_Hashed_RefreshToken()
    {
        await using ApplicationDbContext dbContext = CreateDbContext();
        AuthService authService = CreateAuthService(dbContext, out TokenService tokenService);

        AuthResponse response = await authService.RegisterAsync(new RegisterRequest
        {
            Username = "  MinhAnh  ",
            Password = "StrongPassword123!"
        });

        Assert.Equal("MinhAnh", response.Account.Username);
        Assert.Equal(AccountRole.User, response.Account.Role);
        Assert.NotEmpty(response.AccessToken);
        Assert.NotEmpty(response.RefreshToken);

        Account storedAccount = await dbContext.Accounts.SingleAsync();
        Assert.Equal("MinhAnh", storedAccount.Username);
        Assert.NotEqual("StrongPassword123!", storedAccount.PasswordHash);

        RefreshToken storedToken = await dbContext.RefreshTokens.SingleAsync();
        Assert.NotEqual(response.RefreshToken, storedToken.TokenHash);
        Assert.Equal(tokenService.HashRefreshToken(response.RefreshToken), storedToken.TokenHash);
    }

    [Fact]
    public async Task Jwt_AccessToken_Contains_Only_Allowed_Claims_And_Never_Exposes_Numeric_Database_Id()
    {
        await using ApplicationDbContext dbContext = CreateDbContext();
        AuthService authService = CreateAuthService(dbContext, out _);

        AuthResponse response = await authService.RegisterAsync(new RegisterRequest
        {
            Username = "alice_dev",
            Password = "StrongPassword123!"
        });

        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        string[] claimTypes = jwt.Claims.Select(claim => claim.Type).ToArray();

        Assert.Contains(JwtRegisteredClaimNames.Sub, claimTypes);
        Assert.Contains(ClaimTypes.Name, claimTypes);
        Assert.Contains(ClaimTypes.Role, claimTypes);
        Assert.Contains(JwtRegisteredClaimNames.Jti, claimTypes);

        // BE.md: "Never put the internal numeric database ID in JWT claims. The current sub is username"
        string subClaim = jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
        Assert.Equal("alice_dev", subClaim);
        Assert.DoesNotContain(jwt.Claims, claim => claim.Value == response.Account.Id.ToString());
    }

    [Fact]
    public async Task RefreshToken_Rotation_Revokes_Old_Token_Records_Replacement_Hash_And_Blocks_Reuse()
    {
        await using ApplicationDbContext dbContext = CreateDbContext();
        AuthService authService = CreateAuthService(dbContext, out TokenService tokenService);

        AuthResponse initialSession = await authService.RegisterAsync(new RegisterRequest
        {
            Username = "bob_builder",
            Password = "StrongPassword123!"
        });

        AuthResponse rotatedSession = await authService.RefreshTokenAsync(initialSession.RefreshToken);

        Assert.NotEqual(initialSession.RefreshToken, rotatedSession.RefreshToken);

        string oldHash = tokenService.HashRefreshToken(initialSession.RefreshToken);
        string newHash = tokenService.HashRefreshToken(rotatedSession.RefreshToken);

        RefreshToken oldTokenEntity = await dbContext.RefreshTokens.SingleAsync(token => token.TokenHash == oldHash);
        Assert.NotNull(oldTokenEntity.RevokedAt);
        Assert.Equal(newHash, oldTokenEntity.ReplacedByTokenHash);

        // Reusing the old rotated token must fail and immediately revoke the replacement token (Replay detection)
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.RefreshTokenAsync(initialSession.RefreshToken));

        RefreshToken compromisedReplacementToken = await dbContext.RefreshTokens
            .SingleAsync(token => token.TokenHash == newHash);
        Assert.NotNull(compromisedReplacementToken.RevokedAt);
    }

    [Fact]
    public async Task LoginAsync_Returns_Identical_Error_For_NonExistent_User_And_Wrong_Password()
    {
        await using ApplicationDbContext dbContext = CreateDbContext();
        AuthService authService = CreateAuthService(dbContext, out _);

        await authService.RegisterAsync(new RegisterRequest
        {
            Username = "existing_user",
            Password = "CorrectPassword123!"
        });

        UnauthorizedAccessException missingUserEx = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginRequest
            {
                Username = "unknown_user",
                Password = "CorrectPassword123!"
            }));

        UnauthorizedAccessException wrongPassEx = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginRequest
            {
                Username = "existing_user",
                Password = "WrongPassword123!"
            }));

        // BE.md: "Authentication failures must not reveal whether an account exists."
        Assert.Equal(missingUserEx.Message, wrongPassEx.Message);
    }

    [Fact]
    public async Task SoftDelete_Sets_IsDeleted_And_Filters_Entity_From_Standard_Queries()
    {
        await using ApplicationDbContext dbContext = CreateDbContext();

        var account = new Account
        {
            Username = "temp_account",
            PasswordHash = "hashed"
        };
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        Assert.NotEqual(default, account.CreatedAt);
        Assert.False(account.IsDeleted);

        dbContext.Accounts.Remove(account);
        await dbContext.SaveChangesAsync();

        // Standard query must filter soft-deleted entity
        Assert.Null(await dbContext.Accounts.SingleOrDefaultAsync(candidate => candidate.Id == account.Id));

        // IgnoreQueryFilters must still find the soft-deleted entity with DeletedAt timestamp
        Account? deletedEntity = await dbContext.Accounts
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(candidate => candidate.Id == account.Id);

        Assert.NotNull(deletedEntity);
        Assert.True(deletedEntity.IsDeleted);
        Assert.NotNull(deletedEntity.DeletedAt);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AuthService CreateAuthService(
        ApplicationDbContext dbContext,
        out TokenService tokenService)
    {
        IOptions<JwtSettings> jwtOptions = Options.Create(DefaultJwtSettings);
        var passwordService = new PasswordService();
        tokenService = new TokenService(jwtOptions);
        var refreshTokenService = new RefreshTokenService(dbContext, tokenService, jwtOptions);

        return new AuthService(dbContext, passwordService, tokenService, refreshTokenService);
    }
}

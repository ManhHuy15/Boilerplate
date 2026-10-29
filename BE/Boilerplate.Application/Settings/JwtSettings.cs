namespace Boilerplate.Application.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;
    public string RefreshCookieName { get; init; } = "boilerplate_refresh_token";
    public bool RefreshCookieSecure { get; init; } = true;
    public string RefreshCookieSameSite { get; init; } = "Strict";
}

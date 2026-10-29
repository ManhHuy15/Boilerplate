using System.Text.Json.Serialization;
using Boilerplate.Domain.Enums;

namespace Boilerplate.Application.Services.AuthServices.DTOs;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public AccountResponse Account { get; init; } = null!;

    [JsonIgnore]
    public string RefreshToken { get; init; } = string.Empty;

    [JsonIgnore]
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}

public sealed record AccountResponse(
    int Id,
    string Username,
    AccountRole Role);

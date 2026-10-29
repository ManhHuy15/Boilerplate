using Boilerplate.Domain.Entities;

namespace Boilerplate.Application.Services.RefreshTokenServices.DTOs;

public sealed record RefreshTokenResult(
    string Token,
    DateTime ExpiresAtUtc,
    Account Account);

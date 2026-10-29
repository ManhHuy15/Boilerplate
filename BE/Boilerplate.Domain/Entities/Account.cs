using Boilerplate.Domain.Enums;

namespace Boilerplate.Domain.Entities;

public sealed class Account : BaseEntity
{
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public AccountRole Role { get; set; } = AccountRole.User;

    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

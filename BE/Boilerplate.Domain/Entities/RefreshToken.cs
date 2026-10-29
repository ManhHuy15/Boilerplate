namespace Boilerplate.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public int AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public bool IsActive(DateTime utcNow) =>
        RevokedAt is null && ExpiresAt > utcNow;
}

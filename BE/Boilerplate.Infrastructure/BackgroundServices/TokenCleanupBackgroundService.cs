using Boilerplate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Boilerplate.Infrastructure.BackgroundServices;

public sealed class TokenCleanupBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TokenCleanupBackgroundService> _logger;

    public TokenCleanupBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<TokenCleanupBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PruneExpiredTokensAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to prune expired or revoked refresh tokens.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private async Task PruneExpiredTokensAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        DateTime cutoffUtc = DateTime.UtcNow - RetentionPeriod;

        int deletedCount = await dbContext.RefreshTokens
            .IgnoreQueryFilters()
            .Where(token =>
                (token.RevokedAt != null || token.ExpiresAt < DateTime.UtcNow) &&
                token.CreatedAt < cutoffUtc)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
        {
            _logger.LogInformation("Pruned {Count} expired or revoked refresh tokens.", deletedCount);
        }
    }
}

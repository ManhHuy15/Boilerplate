using Boilerplate.Application.InfraInterfaces;
using Boilerplate.Domain;
using Boilerplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Boilerplate.Infrastructure.Data;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    private const string SoftDeletionFilter = "SoftDeletionFilter";

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.BaseType is not null ||
                !typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            typeof(ApplicationDbContext)
                .GetMethod(
                    nameof(ApplySoftDeleteFilter),
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [modelBuilder]);
        }
    }

    private void ApplyAuditAndSoftDelete()
    {
        DateTime utcNow = DateTime.UtcNow;

        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = null;
                    entry.Entity.IsDeleted = false;
                    entry.Entity.DeletedAt = null;
                    break;

                case EntityState.Modified:
                    entry.Property(entity => entity.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Deleted:
                    ApplySoftDelete(entry, utcNow);
                    break;
            }
        }
    }

    private static void ApplySoftDelete(
        EntityEntry<BaseEntity> entry,
        DateTime utcNow)
    {
        entry.State = EntityState.Unchanged;

        entry.Property(entity => entity.IsDeleted).CurrentValue = true;
        entry.Property(entity => entity.IsDeleted).IsModified = true;

        entry.Property(entity => entity.DeletedAt).CurrentValue = utcNow;
        entry.Property(entity => entity.DeletedAt).IsModified = true;

        entry.Property(entity => entity.UpdatedAt).CurrentValue = utcNow;
        entry.Property(entity => entity.UpdatedAt).IsModified = true;
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(
                SoftDeletionFilter,
                entity => !entity.IsDeleted);
    }
}

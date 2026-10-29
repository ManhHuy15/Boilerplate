using Boilerplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boilerplate.Infrastructure.Data.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Username)
            .HasColumnType("citext")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(account => account.PasswordHash).HasMaxLength(500).IsRequired();
        builder.HasIndex(account => account.Username).IsUnique();
    }
}

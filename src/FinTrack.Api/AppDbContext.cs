using FinTrack.Api.Transactions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api;

/// <summary>
/// Application EF Core DbContext.
/// Includes Identity tables and transaction entities.
/// </summary>
public sealed class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Transaction records owned by users.
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.UserId)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Amount)
                .HasPrecision(18, 2);

            entity.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(t => t.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => new { t.UserId, t.CreatedAtUtc });
        });
    }
}
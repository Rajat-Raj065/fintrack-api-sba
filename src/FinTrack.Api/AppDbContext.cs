using FinTrack.Api.Expenses.Models;
using FinTrack.Api.Transactions.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api;

/// <summary>
/// Application EF Core DbContext.
/// Includes Identity tables and domain entities.
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

    /// <summary>
    /// Shared expense records.
    /// </summary>
    public DbSet<SharedExpense> SharedExpenses => Set<SharedExpense>();

    /// <summary>
    /// Shared expense participant records.
    /// </summary>
    public DbSet<SharedExpenseParticipant> SharedExpenseParticipants => Set<SharedExpenseParticipant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.UserId).IsRequired().HasMaxLength(128);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            entity.Property(t => t.CreatedAtUtc).IsRequired();

            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => new { t.UserId, t.CreatedAtUtc });
        });

        builder.Entity<SharedExpense>(entity =>
        {
            entity.ToTable("SharedExpenses");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CreatorUserId).IsRequired().HasMaxLength(128);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(200);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.SplitType).IsRequired().HasMaxLength(10);
            entity.Property(x => x.CreatedAtUtc).IsRequired();

            entity.HasMany(x => x.Participants)
                .WithOne(x => x.SharedExpense)
                .HasForeignKey(x => x.SharedExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.CreatorUserId);
        });

        builder.Entity<SharedExpenseParticipant>(entity =>
        {
            entity.ToTable("SharedExpenseParticipants");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ParticipantUserId).IsRequired().HasMaxLength(128);
            entity.Property(x => x.ShareAmount).HasPrecision(18, 2);

            entity.HasIndex(x => x.ParticipantUserId);
            entity.HasIndex(x => new { x.SharedExpenseId, x.ParticipantUserId }).IsUnique();
        });
    }
}
using FinancePlanner.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FinancePlanner.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            e.HasIndex(u => u.Email).IsUnique();
        });

        builder.Entity<Account>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).IsRequired().HasMaxLength(100);
            e.Property(a => a.Currency).IsRequired().HasMaxLength(3);
            e.Property(a => a.Balance).HasPrecision(18, 4);
            e.HasOne(a => a.User)
             .WithMany(u => u.Accounts)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasPrecision(18, 4);
            e.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            e.Property(t => t.CategoryName).IsRequired().HasMaxLength(50);
            e.Property(t => t.Description).HasMaxLength(500);
            e.HasIndex(t => new { t.AccountId, t.OccurredOn });
            e.HasIndex(t => t.CategoryName);
            e.HasOne(t => t.Account)
             .WithMany(a => a.Transactions)
             .HasForeignKey(t => t.AccountId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Budget>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.CategoryName).IsRequired().HasMaxLength(50);
            e.Property(b => b.Currency).IsRequired().HasMaxLength(3);
            e.Property(b => b.LimitAmount).HasPrecision(18, 4);
            e.HasOne(b => b.User)
             .WithMany()
             .HasForeignKey(b => b.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
            e.Property(a => a.Action).IsRequired().HasMaxLength(50);
            e.Property(a => a.OldValues).HasMaxLength(4000);
            e.Property(a => a.NewValues).HasMaxLength(4000);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog
                && e.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted)
            .ToList();

        foreach (var entry in auditEntries)
        {
            AuditLogs.Add(new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = entry.State.ToString(),
                Timestamp = DateTime.UtcNow,
                OldValues = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                    ? System.Text.Json.JsonSerializer.Serialize(
                        entry.OriginalValues.Properties.ToDictionary(
                            p => p.Name,
                            p => entry.OriginalValues[p]?.ToString()))
                    : null,
                NewValues = entry.State == EntityState.Added || entry.State == EntityState.Modified
                    ? System.Text.Json.JsonSerializer.Serialize(
                        entry.CurrentValues.Properties.ToDictionary(
                            p => p.Name,
                            p => entry.CurrentValues[p]?.ToString()))
                    : null
            });
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private int GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties
            .FirstOrDefault(p => p.Metadata.IsPrimaryKey());

        return idProperty?.CurrentValue is int id ? id : 0;
    }
}
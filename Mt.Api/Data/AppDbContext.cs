using Microsoft.EntityFrameworkCore;
using Mt.Api.Tenancy;

namespace Mt.Api.Data;

public sealed class AppDbContext : DbContext
{
    private readonly TenantContext _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, TenantContext tenant)
        : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var todo = modelBuilder.Entity<TodoItem>();

        todo.HasKey(x => x.Id);

        todo.Property(x => x.TenantId)
            .HasMaxLength(64)
            .IsRequired();

        todo.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        // Helpful for common tenant queries
        todo.HasIndex(x => new { x.TenantId, x.CreatedUtc });

        // Read isolation layer: every query automatically scopes by tenant.
        todo.HasQueryFilter(x => x.TenantId == _tenant.TenantId);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceTenantOnWrites();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnforceTenantOnWrites();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnforceTenantOnWrites()
    {
        if (!_tenant.IsSet)
            throw new InvalidOperationException("TenantContext not set. Refusing to write.");

        foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
        {
            if (entry.State == EntityState.Added)
            {
                // Force correct tenant on inserts (overwrites any client-supplied value).
                entry.Entity.TenantId = _tenant.TenantId;
            }
            else if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                // Fail closed on cross-tenant write attempts.
                if (!string.Equals(entry.Entity.TenantId, _tenant.TenantId, StringComparison.Ordinal))
                    throw new UnauthorizedAccessException("Cross-tenant write blocked.");
            }
        }
    }
}

using System.Text.Json;
using Media.Domain.Entities;
using Media.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Data;

public class MediaDbContext : DbContext, IUnitOfWork
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options) { }

    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<MediaVersion> MediaVersions => Set<MediaVersion>();
    public DbSet<MediaFolder> MediaFolders => Set<MediaFolder>();
    public DbSet<MediaTag> MediaTags => Set<MediaTag>();
    public DbSet<MediaItemTag> MediaItemTags => Set<MediaItemTag>();
    public DbSet<MediaShare> MediaShares => Set<MediaShare>();
    public DbSet<MediaAuditLog> MediaAuditLogs => Set<MediaAuditLog>();
    public DbSet<MediaSearchIndex> MediaSearchIndices => Set<MediaSearchIndex>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        AutoAudit();
        return await base.SaveChangesAsync(ct);
    }

    private void AutoAudit()
    {
        foreach (var entry in ChangeTracker.Entries<MediaItem>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(MediaItem.CreatedAt)).IsModified = false;
                entry.Property(nameof(MediaItem.CreatedBy)).IsModified = false;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}

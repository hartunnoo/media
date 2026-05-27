using Media.Domain.Entities;
using Media.Domain.Interfaces;
using Media.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Media.Infrastructure.Repositories;

public class MediaRepository(MediaDbContext context) : IMediaRepository
{
    public async Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.MediaItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<MediaItem>> ListAsync(string? search, Guid? folderId, string? ownedByUserId, int skip, int take, CancellationToken ct = default)
    {
        var query = context.MediaItems.AsNoTracking().Where(x => x.Status != Domain.Enums.MediaStatus.SoftDeleted);
        if (folderId.HasValue) query = query.Where(x => x.FolderId == folderId.Value);
        if (!string.IsNullOrWhiteSpace(ownedByUserId)) query = query.Where(x => x.OwnedByUserId == ownedByUserId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.OriginalFileName.Contains(search));
        return await query.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<int> CountAsync(string? search, Guid? folderId, string? ownedByUserId, CancellationToken ct = default)
    {
        var query = context.MediaItems.AsNoTracking().Where(x => x.Status != Domain.Enums.MediaStatus.SoftDeleted);
        if (folderId.HasValue) query = query.Where(x => x.FolderId == folderId.Value);
        if (!string.IsNullOrWhiteSpace(ownedByUserId)) query = query.Where(x => x.OwnedByUserId == ownedByUserId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.OriginalFileName.Contains(search));
        return await query.CountAsync(ct);
    }

    public async Task AddAsync(MediaItem entity, CancellationToken ct = default) => await context.MediaItems.AddAsync(entity, ct);
    public void Update(MediaItem entity) => context.MediaItems.Update(entity);
    public void Delete(MediaItem entity) => context.MediaItems.Remove(entity);

    public async Task<MediaVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken ct = default)
        => await context.MediaVersions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == versionId, ct);

    public async Task<IReadOnlyList<MediaVersion>> GetVersionsAsync(Guid mediaId, CancellationToken ct = default)
        => await context.MediaVersions.AsNoTracking().Where(x => x.MediaId == mediaId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public async Task AddVersionAsync(MediaVersion version, CancellationToken ct = default) => await context.MediaVersions.AddAsync(version, ct);

    public async Task PruneVersionsAsync(Guid mediaId, int keepCount, CancellationToken ct = default)
    {
        var toDelete = await context.MediaVersions.Where(x => x.MediaId == mediaId)
            .OrderByDescending(x => x.CreatedAt).Skip(keepCount).Select(x => x.Id).ToListAsync(ct);
        if (toDelete.Count > 0)
            await context.MediaVersions.Where(x => toDelete.Contains(x.Id)).ExecuteDeleteAsync(ct);
    }

    public async Task<string?> GetFileHashAsync(string hash, CancellationToken ct = default)
        => await context.MediaItems.Where(x => x.FileHash == hash).Select(x => x.OriginalFileName).FirstOrDefaultAsync(ct);
}

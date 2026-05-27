using Media.Domain.Entities;

namespace Media.Domain.Interfaces;

public interface IMediaRepository
{
    Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MediaItem>> ListAsync(string? search, Guid? folderId, string? ownedByUserId, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(string? search, Guid? folderId, string? ownedByUserId, CancellationToken ct = default);
    Task AddAsync(MediaItem entity, CancellationToken ct = default);
    void Update(MediaItem entity);
    void Delete(MediaItem entity);
    Task<MediaVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken ct = default);
    Task<IReadOnlyList<MediaVersion>> GetVersionsAsync(Guid mediaId, CancellationToken ct = default);
    Task AddVersionAsync(MediaVersion version, CancellationToken ct = default);
    Task PruneVersionsAsync(Guid mediaId, int keepCount, CancellationToken ct = default);
    Task<string?> GetFileHashAsync(string hash, CancellationToken ct = default);
}

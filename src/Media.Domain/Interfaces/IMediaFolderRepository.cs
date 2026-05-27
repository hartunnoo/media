using Media.Domain.Entities;

namespace Media.Domain.Interfaces;

public interface IMediaFolderRepository
{
    Task<MediaFolder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MediaFolder>> ListAsync(Guid? parentId, string? ownedByUserId, CancellationToken ct = default);
    Task AddAsync(MediaFolder entity, CancellationToken ct = default);
    void Update(MediaFolder entity);
    void Delete(MediaFolder entity);
}

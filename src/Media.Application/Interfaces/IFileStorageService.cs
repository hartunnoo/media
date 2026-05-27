namespace Media.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Guid mediaId, Stream stream, string extension, CancellationToken ct = default);
    Task<Stream?> GetStreamAsync(Guid mediaId, string storedFileName, CancellationToken ct = default);
    Task DeleteAsync(Guid mediaId, string storedFileName, CancellationToken ct = default);
    string GetStoragePath(Guid mediaId, string storedFileName);
}

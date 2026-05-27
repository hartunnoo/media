using Media.Application.Interfaces;

namespace Media.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Guid mediaId, Stream stream, string extension, CancellationToken ct = default)
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyy/MM");
        var dir = Path.Combine(_basePath, yearMonth);
        Directory.CreateDirectory(dir);
        var storedName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
        var path = Path.Combine(dir, storedName);
        await using var fs = File.Create(path);
        await stream.CopyToAsync(fs, ct);
        return storedName;
    }

    public Task<Stream?> GetStreamAsync(Guid mediaId, string storedFileName, CancellationToken ct = default)
    {
        var path = GetStoragePath(mediaId, storedFileName);
        return File.Exists(path) ? Task.FromResult<Stream?>(File.OpenRead(path)) : Task.FromResult<Stream?>(null);
    }

    public Task DeleteAsync(Guid mediaId, string storedFileName, CancellationToken ct = default)
    {
        var path = GetStoragePath(mediaId, storedFileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public string GetStoragePath(Guid mediaId, string storedFileName) => Path.Combine(_basePath, storedFileName);
}

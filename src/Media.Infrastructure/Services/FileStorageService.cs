using Media.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Media.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _basePath = configuration.GetValue<string>("Storage:BasePath")
            ?? Path.Combine(env.WebRootPath ?? env.ContentRootPath, "media");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Guid mediaId, Stream stream, string extension, CancellationToken ct = default)
    {
        var ext = NormalizeExtension(extension);
        var now = DateTime.UtcNow;
        var dir = Path.Combine(_basePath, now.ToString("yyyy"), now.ToString("MM"), mediaId.ToString("N"));
        Directory.CreateDirectory(dir);

        var storedName = $"original{ext}";
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
        // Delete the entire media folder (original + thumbnails)
        var dir = GetMediaDirectory(mediaId);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, recursive: true);
        }
        return Task.CompletedTask;
    }

    public string GetStoragePath(Guid mediaId, string storedFileName)
        => Path.Combine(GetMediaDirectory(mediaId), storedFileName);

    public string GetThumbnailDirectory(Guid mediaId)
        => Path.Combine(GetMediaDirectory(mediaId), "thumbnails");

    private string GetMediaDirectory(Guid mediaId)
    {
        // Find the media folder by searching year/month directories
        var years = Directory.GetDirectories(_basePath);
        foreach (var yearDir in years)
        {
            var months = Directory.GetDirectories(yearDir);
            foreach (var monthDir in months)
            {
                var mediaDir = Path.Combine(monthDir, mediaId.ToString("N"));
                if (Directory.Exists(mediaDir)) return mediaDir;
            }
        }
        // Fallback if not found (for new saves)
        return Path.Combine(_basePath, DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), mediaId.ToString("N"));
    }

    private static string NormalizeExtension(string ext)
    {
        ext = (ext ?? ".bin").ToLowerInvariant().Trim();
        if (!ext.StartsWith('.')) ext = "." + ext;
        // Normalize common variants
        return ext switch
        {
            ".jpeg" => ".jpg",
            ".svg+xml" => ".svg",
            _ => ext
        };
    }
}

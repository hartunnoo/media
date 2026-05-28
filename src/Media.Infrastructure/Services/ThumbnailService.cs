using Media.Application.Interfaces;
using Media.Domain.Constants;
using SkiaSharp;

namespace Media.Infrastructure.Services;

public class ThumbnailService(IFileStorageService fileStorage) : IThumbnailService
{
    public Task<Dictionary<string, byte[]>> GenerateAsync(Stream imageStream, CancellationToken ct = default)
    {
        var result = new Dictionary<string, byte[]>();
        imageStream.Position = 0;
        using var bitmap = SKBitmap.Decode(imageStream);
        if (bitmap is null) return Task.FromResult(result);

        var sizes = new Dictionary<string, int>
        {
            ["tiny"] = MediaConstants.ThumbnailSizes.Tiny,
            ["small"] = MediaConstants.ThumbnailSizes.Small,
            ["medium"] = MediaConstants.ThumbnailSizes.Medium,
            ["large"] = MediaConstants.ThumbnailSizes.Large
        };

        foreach (var (key, size) in sizes)
        {
            int w = size, h = size;
            if (bitmap.Width > bitmap.Height) h = (int)(bitmap.Height * (size / (float)bitmap.Width));
            else w = (int)(bitmap.Width * (size / (float)bitmap.Height));

            using var resized = bitmap.Resize(new SKImageInfo(w, h), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Webp, 85);
            result[key] = data.ToArray();
        }
        return Task.FromResult(result);
    }

    public async Task SaveThumbnailsAsync(Guid mediaId, Dictionary<string, byte[]> thumbnails, CancellationToken ct = default)
    {
        var dir = (fileStorage as Services.FileStorageService)!.GetThumbnailDirectory(mediaId);
        Directory.CreateDirectory(dir);
        foreach (var (size, bytes) in thumbnails)
            await File.WriteAllBytesAsync(Path.Combine(dir, $"{size}.webp"), bytes, ct);
    }

    public Task<byte[]?> GetThumbnailAsync(Guid mediaId, string size, CancellationToken ct = default)
    {
        var dir = (fileStorage as Services.FileStorageService)!.GetThumbnailDirectory(mediaId);
        var path = Path.Combine(dir, $"{size}.webp");
        return File.Exists(path) ? Task.FromResult<byte[]?>(File.ReadAllBytes(path)) : Task.FromResult<byte[]?>(null);
    }
}

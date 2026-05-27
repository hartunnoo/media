using Media.Application.Interfaces;
using Media.Domain.Constants;
using SkiaSharp;

namespace Media.Infrastructure.Services;

public class ThumbnailService : IThumbnailService
{
    private readonly string _thumbDir;

    public ThumbnailService()
    {
        _thumbDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "thumbnails");
        Directory.CreateDirectory(_thumbDir);
    }

    public Task<Dictionary<string, byte[]>> GenerateAsync(Stream imageStream, CancellationToken ct = default)
    {
        var result = new Dictionary<string, byte[]>();
        imageStream.Position = 0;
        using var bitmap = SKBitmap.Decode(imageStream);
        if (bitmap is null) return Task.FromResult(result);

        var sizes = new Dictionary<string, int> { ["tiny"] = MediaConstants.ThumbnailSizes.Tiny, ["small"] = MediaConstants.ThumbnailSizes.Small, ["medium"] = MediaConstants.ThumbnailSizes.Medium, ["large"] = MediaConstants.ThumbnailSizes.Large };
        foreach (var (key, size) in sizes)
        {
            int w = size, h = size;
            if (bitmap.Width > bitmap.Height) h = (int)(bitmap.Height * (size / (float)bitmap.Width));
            else w = (int)(bitmap.Width * (size / (float)bitmap.Height));

            using var resized = bitmap.Resize(new SKImageInfo(w, h), SKFilterQuality.Medium);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Webp, 85);
            result[key] = data.ToArray();
        }
        return Task.FromResult(result);
    }

    public Task<byte[]?> GetThumbnailAsync(Guid mediaId, string size, CancellationToken ct = default)
    {
        var path = Path.Combine(_thumbDir, mediaId.ToString(), $"{size}.webp");
        return File.Exists(path) ? Task.FromResult<byte[]?>(File.ReadAllBytes(path)) : Task.FromResult<byte[]?>(null);
    }
}

using Media.Application.Interfaces;
using Media.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Media.Infrastructure.Services;

/// <summary>
/// Hangfire-driven thumbnail warmup — scans for items missing thumbnails and generates them.
/// Called by recurring job every 30 minutes.
/// </summary>
public sealed class ThumbnailWarmupService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ThumbnailWarmupService> _logger;
    private const int BatchSize = 50;

    public ThumbnailWarmupService(IServiceProvider services, ILogger<ThumbnailWarmupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task RunWarmupAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMediaRepository>();
        var thumbnailService = scope.ServiceProvider.GetRequiredService<IThumbnailService>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var ids = await repository.GetIdsWithoutThumbnailsAsync(BatchSize, ct);

        if (ids.Count == 0) return;

        _logger.LogInformation("Thumbnail warmup: found {Count} items needing thumbnails", ids.Count);
        var generated = 0;

        foreach (var id in ids)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var item = await repository.GetByIdAsync(id, ct);
                if (item is null) continue;

                await using var stream = await fileStorage.GetStreamAsync(id, item.StoredFileName, ct);
                if (stream is null) continue;

                // Buffer into bytes: SKBitmap.Decode closes the stream
                var buffer = new byte[item.FileSize > 0 ? item.FileSize : stream.Length];
                await stream.ReadExactlyAsync(buffer, ct);

                using var probe = SkiaSharp.SKBitmap.Decode(buffer);
                if (probe is null) continue;

                using var ms = new MemoryStream(buffer);
                var thumbnails = await thumbnailService.GenerateAsync(ms, ct);
                if (thumbnails.Count > 0)
                {
                    await ((ThumbnailService)thumbnailService).SaveThumbnailsAsync(id, thumbnails, ct);
                    generated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Thumbnail warmup failed for {MediaId}", id);
            }
        }

        if (generated > 0)
            _logger.LogInformation("Thumbnail warmup: generated thumbnails for {Count} items", generated);
    }
}

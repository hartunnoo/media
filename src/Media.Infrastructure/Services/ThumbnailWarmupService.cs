using Media.Application.Interfaces;
using Media.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Media.Infrastructure.Services;

/// <summary>
/// Periodically scans for image media items that lack thumbnails and generates them.
/// Runs every 30 minutes to avoid competing with the Hangfire processing queue.
/// </summary>
public sealed class ThumbnailWarmupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ThumbnailWarmupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);
    private const int BatchSize = 50;

    public ThumbnailWarmupService(IServiceProvider services, ILogger<ThumbnailWarmupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Delay first run to let the app fully start
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Thumbnail warmup cycle failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
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
                if (item is null || !item.ContentType.StartsWith("image/")) continue;

                await using var stream = await fileStorage.GetStreamAsync(id, item.StoredFileName, ct);
                if (stream is null) continue;

                var thumbnails = await thumbnailService.GenerateAsync(stream, ct);
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

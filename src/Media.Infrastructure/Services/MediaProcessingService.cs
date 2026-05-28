using System.Security.Cryptography;
using Media.Application.Interfaces;
using Media.Domain.Enums;
using Media.Domain.Interfaces;
using Media.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Media.Infrastructure.Services;

public class MediaProcessingService(
    IMediaRepository repository,
    IFileStorageService fileStorage,
    IThumbnailService thumbnailService,
    IUnitOfWork unitOfWork,
    IHubContext<MediaHub> hubContext,
    ILogger<MediaProcessingService> logger) : IMediaProcessingService
{
    public async Task ProcessAsync(Guid mediaId, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(mediaId, ct);
        if (item is null) return;

        try
        {
            item.Status = MediaStatus.Processing;
            repository.Update(item);
            await unitOfWork.SaveChangesAsync(ct);
            await hubContext.Clients.Group(mediaId.ToString()).SendAsync("StatusChanged", new { mediaId, status = "Processing" }, ct);

            // SHA-256 hash
            await using var stream = await fileStorage.GetStreamAsync(mediaId, item.StoredFileName, ct);
            if (stream is not null)
            {
                var hash = await ComputeHashAsync(stream, ct);
                item.FileHash = hash;

                // Duplicate check
                var existingName = await repository.GetFileHashAsync(hash, ct);
                if (existingName is not null)
                    logger.LogInformation("Duplicate detected: {MediaId} matches {ExistingName}", mediaId, existingName);

                // Generate thumbnails
                if (item.ContentType.StartsWith("image/"))
                {
                    stream.Position = 0;
                    var thumbnails = await thumbnailService.GenerateAsync(stream, ct);
                    await (thumbnailService as ThumbnailService)!.SaveThumbnailsAsync(mediaId, thumbnails, ct);
                }
            }

            item.Status = MediaStatus.Available;
            repository.Update(item);
            await unitOfWork.SaveChangesAsync(ct);
            await hubContext.Clients.Group(mediaId.ToString()).SendAsync("StatusChanged", new { mediaId, status = "Available" }, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Processing failed for {MediaId}", mediaId);
            item.Status = MediaStatus.Failed;
            item.ErrorMessage = ex.Message;
            repository.Update(item);
            await unitOfWork.SaveChangesAsync(ct);
            await hubContext.Clients.Group(mediaId.ToString()).SendAsync("StatusChanged", new { mediaId, status = "Failed", error = ex.Message }, ct);
        }
    }

    private static async Task<string> ComputeHashAsync(Stream stream, CancellationToken ct)
    {
        stream.Position = 0;
        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexStringLower(hash);
    }
}

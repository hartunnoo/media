using Media.Application.Commands.UploadMedia;
using Media.Application.DTOs;
using Media.Application.Interfaces;
using Media.Domain.Entities;
using Media.Domain.Enums;
using Media.Domain.Interfaces;
using MediatR;
using Hangfire;

namespace Media.Infrastructure.Handlers;

public class UploadMediaCommandHandler(
    IMediaRepository repository, IFileStorageService fileStorage, IUnitOfWork unitOfWork)
    : IRequestHandler<UploadMediaCommand, UploadResultDto>
{
    public async Task<UploadResultDto> Handle(UploadMediaCommand request, CancellationToken ct)
    {
        var ext = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();

        var item = new MediaItem
        {
            OriginalFileName = request.OriginalFileName,
            ContentType = request.ContentType,
            FileSize = request.FileStream.Length,
            OwnedByUserId = request.OwnedByUserId,
            OwnedByAppId = request.OwnedByAppId,
            FolderId = request.FolderId,
            CreatedBy = request.OwnedByUserId,
            UpdatedBy = request.OwnedByUserId
        };

        var storedName = await fileStorage.SaveAsync(item.Id, request.FileStream, ext, ct);
        item.StoredFileName = storedName;
        item.StoragePath = fileStorage.GetStoragePath(item.Id, storedName);

        await repository.AddAsync(item, ct);

        await repository.AddAuditLogAsync(new MediaAuditLog
        {
            MediaId = item.Id,
            Action = MediaActionType.Create,
            UserId = request.OwnedByUserId
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);

        try { BackgroundJob.Enqueue<IMediaProcessingService>(s => s.ProcessAsync(item.Id, CancellationToken.None)); }
        catch (InvalidOperationException) { /* Hangfire not configured */ }

        return new UploadResultDto(item.Id, Domain.Enums.MediaStatus.Queued, "File queued for processing");
    }
}

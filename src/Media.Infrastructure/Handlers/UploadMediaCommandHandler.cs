using Media.Application.Commands.UploadMedia;
using Media.Application.DTOs;
using Media.Application.Interfaces;
using Media.Domain.Entities;
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
        var storedName = await fileStorage.SaveAsync(Guid.NewGuid(), request.FileStream, ext, ct);

        var item = new MediaItem
        {
            OriginalFileName = request.OriginalFileName, StoredFileName = storedName,
            ContentType = request.ContentType, FileSize = request.FileStream.Length,
            StoragePath = fileStorage.GetStoragePath(Guid.Empty, storedName),
            OwnedByUserId = request.OwnedByUserId, OwnedByAppId = request.OwnedByAppId,
            FolderId = request.FolderId, CreatedBy = request.OwnedByUserId, UpdatedBy = request.OwnedByUserId
        };

        await repository.AddAsync(item, ct);
        await unitOfWork.SaveChangesAsync(ct);

        BackgroundJob.Enqueue<IMediaProcessingService>(s => s.ProcessAsync(item.Id, CancellationToken.None));

        return new UploadResultDto(item.Id, Domain.Enums.MediaStatus.Queued, "File queued for processing");
    }
}

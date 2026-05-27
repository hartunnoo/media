using Media.Application.Commands.SwapMedia;
using Media.Application.Interfaces;
using Media.Domain.Entities;
using Media.Domain.Interfaces;
using MediatR;

namespace Media.Infrastructure.Handlers;

public class SwapMediaCommandHandler(
    IMediaRepository repository, IFileStorageService fileStorage, IUnitOfWork unitOfWork)
    : IRequestHandler<SwapMediaCommand>
{
    public async Task Handle(SwapMediaCommand request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(request.MediaId, ct);
        if (item is null) return;

        // Save old version
        var version = new MediaVersion
        {
            MediaId = item.Id, FileHash = item.FileHash ?? "", Size = item.FileSize,
            StoragePath = item.StoragePath, Comment = request.Comment, CreatedBy = item.UpdatedBy
        };
        await repository.AddVersionAsync(version, ct);
        await repository.PruneVersionsAsync(item.Id, Domain.Constants.MediaConstants.MaxVersionRetention, ct);

        // Save new file
        var ext = Path.GetExtension(item.OriginalFileName);
        var storedName = await fileStorage.SaveAsync(item.Id, request.NewFileStream, ext, ct);
        item.StoredFileName = storedName;
        item.FileSize = request.NewFileStream.Length;
        item.ContentType = request.NewContentType;
        item.StoragePath = fileStorage.GetStoragePath(item.Id, storedName);
        repository.Update(item);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

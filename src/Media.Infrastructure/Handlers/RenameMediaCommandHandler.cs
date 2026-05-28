using Media.Application.Commands.RenameMedia;
using Media.Application.Interfaces;
using Media.Domain.Entities;
using Media.Domain.Enums;
using Media.Domain.Interfaces;
using MediatR;

namespace Media.Infrastructure.Handlers;

public class RenameMediaCommandHandler(
    IMediaRepository repository,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorage)
    : IRequestHandler<RenameMediaCommand>
{
    public async Task Handle(RenameMediaCommand request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(request.MediaId, ct);
        if (item is null) return;
        var oldName = item.OriginalFileName;

        // Rename physical file on disk
        var newStoredName = await fileStorage.RenameAsync(item.Id, item.StoredFileName, request.NewFileName, ct);
        item.StoredFileName = newStoredName;
        item.StoragePath = fileStorage.GetStoragePath(item.Id, newStoredName);
        item.OriginalFileName = request.NewFileName;
        repository.Update(item);

        await repository.AddAuditLogAsync(new MediaAuditLog
        {
            MediaId = item.Id,
            Action = MediaActionType.Rename,
            UserId = "system",
            Changes = System.Text.Json.JsonSerializer.Serialize(new { from = oldName, to = request.NewFileName })
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

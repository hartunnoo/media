using Media.Application.Commands.RenameMedia;
using Media.Domain.Entities;
using Media.Domain.Enums;
using Media.Domain.Interfaces;
using MediatR;

namespace Media.Infrastructure.Handlers;

public class RenameMediaCommandHandler(IMediaRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<RenameMediaCommand>
{
    public async Task Handle(RenameMediaCommand request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(request.MediaId, ct);
        if (item is null) return;
        var oldName = item.OriginalFileName;
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

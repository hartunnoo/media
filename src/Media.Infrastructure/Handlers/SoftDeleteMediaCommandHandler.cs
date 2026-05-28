using Media.Application.Commands.SoftDeleteMedia;
using Media.Domain.Entities;
using Media.Domain.Enums;
using Media.Domain.Interfaces;
using MediatR;

namespace Media.Infrastructure.Handlers;

public class SoftDeleteMediaCommandHandler(IMediaRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<SoftDeleteMediaCommand>
{
    public async Task Handle(SoftDeleteMediaCommand request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(request.MediaId, ct);
        if (item is null) return;
        item.Status = MediaStatus.SoftDeleted;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = request.DeletedBy;
        repository.Update(item);

        await repository.AddAuditLogAsync(new MediaAuditLog
        {
            MediaId = item.Id,
            Action = MediaActionType.SoftDelete,
            UserId = request.DeletedBy
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

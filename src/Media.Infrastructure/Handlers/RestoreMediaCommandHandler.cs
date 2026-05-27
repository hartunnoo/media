using Media.Application.Commands.RestoreMedia;
using Media.Domain.Enums;
using Media.Domain.Interfaces;
using MediatR;

namespace Media.Infrastructure.Handlers;

public class RestoreMediaCommandHandler(IMediaRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<RestoreMediaCommand>
{
    public async Task Handle(RestoreMediaCommand request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(request.MediaId, ct);
        if (item is null || item.Status != MediaStatus.SoftDeleted) return;
        item.Status = MediaStatus.Available;
        item.RestoredAt = DateTime.UtcNow;
        item.RestoredBy = request.RestoredBy;
        item.DeletedAt = null; item.DeletedBy = null;
        repository.Update(item);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

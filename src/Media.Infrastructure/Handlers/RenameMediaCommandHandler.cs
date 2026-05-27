using Media.Application.Commands.RenameMedia;
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
        item.OriginalFileName = request.NewFileName;
        repository.Update(item);
        await unitOfWork.SaveChangesAsync(ct);
    }
}

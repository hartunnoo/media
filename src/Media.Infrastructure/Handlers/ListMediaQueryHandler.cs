using Media.Application.DTOs;
using Media.Application.Queries.ListMedia;
using Media.Domain.Interfaces;
using MediatR;

namespace Media.Infrastructure.Handlers;

public class ListMediaQueryHandler(IMediaRepository repository)
    : IRequestHandler<ListMediaQuery, IReadOnlyList<MediaItemDto>>
{
    public async Task<IReadOnlyList<MediaItemDto>> Handle(ListMediaQuery request, CancellationToken ct)
    {
        var items = await repository.ListAsync(request.Search, request.FolderId, request.OwnedByUserId, request.Skip, request.Take, ct);
        return items.Select(item => new MediaItemDto(
            item.Id, item.OriginalFileName, item.ContentType, item.FileSize, item.FileHash,
            item.Status, item.OwnedByUserId, item.OwnedByAppId, item.FolderId,
            null, item.IsLegalHold, item.CreatedAt, item.CreatedBy,
            item.UpdatedAt, item.UpdatedBy, item.DeletedAt)).ToList();
    }
}

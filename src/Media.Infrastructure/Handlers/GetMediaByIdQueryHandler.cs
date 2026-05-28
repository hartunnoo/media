using Media.Application.DTOs;
using Media.Application.Queries.GetMediaById;
using Media.Domain.Interfaces;
using MediatR;

namespace Media.Infrastructure.Handlers;

public class GetMediaByIdQueryHandler(IMediaRepository repository)
    : IRequestHandler<GetMediaByIdQuery, MediaItemDto?>
{
    public async Task<MediaItemDto?> Handle(GetMediaByIdQuery request, CancellationToken ct)
    {
        var item = await repository.GetByIdAsync(request.MediaId, ct);
        if (item is null) return null;

        return new MediaItemDto(
            item.Id, item.OriginalFileName, item.StoredFileName, item.ContentType, item.FileSize, item.FileHash,
            item.Status, item.OwnedByUserId, item.OwnedByAppId, item.FolderId,
            null, item.IsLegalHold, item.CreatedAt, item.CreatedBy,
            item.UpdatedAt, item.UpdatedBy, item.DeletedAt);
    }
}

using Media.Application.DTOs;
using MediatR;

namespace Media.Application.Queries.ListMedia;

public record ListMediaQuery(
    string? Search = null, Guid? FolderId = null, string? OwnedByUserId = null,
    int Skip = 0, int Take = 20) : IRequest<IReadOnlyList<MediaItemDto>>;

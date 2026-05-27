using Media.Application.DTOs;
using MediatR;

namespace Media.Application.Queries.GetMediaById;

public record GetMediaByIdQuery(Guid MediaId) : IRequest<MediaItemDto?>;

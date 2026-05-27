using MediatR;

namespace Media.Application.Commands.SoftDeleteMedia;

public record SoftDeleteMediaCommand(Guid MediaId, string DeletedBy) : IRequest;

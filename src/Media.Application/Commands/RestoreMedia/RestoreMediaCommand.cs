using MediatR;

namespace Media.Application.Commands.RestoreMedia;

public record RestoreMediaCommand(Guid MediaId, string RestoredBy) : IRequest;

using MediatR;

namespace Media.Application.Commands.RenameMedia;

public record RenameMediaCommand(Guid MediaId, string NewFileName) : IRequest;

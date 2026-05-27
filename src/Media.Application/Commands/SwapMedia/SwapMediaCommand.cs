using MediatR;

namespace Media.Application.Commands.SwapMedia;

public record SwapMediaCommand(Guid MediaId, Stream NewFileStream, string NewContentType, string? Comment = null) : IRequest;

using Media.Application.DTOs;
using MediatR;

namespace Media.Application.Commands.UploadMedia;

public record UploadMediaCommand(
    Stream FileStream, string OriginalFileName, string ContentType,
    Guid? FolderId = null, string OwnedByUserId = "", string? OwnedByAppId = null,
    List<string>? Tags = null) : IRequest<UploadResultDto>;

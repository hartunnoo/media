using Media.Domain.Enums;

namespace Media.Application.DTOs;

public record UploadResultDto(Guid MediaId, MediaStatus Status, string Message);

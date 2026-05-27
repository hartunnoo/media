namespace Media.Application.DTOs;

public record MediaVersionDto(Guid Id, Guid MediaId, string FileHash, long Size, string? Comment, DateTime CreatedAt, string CreatedBy);

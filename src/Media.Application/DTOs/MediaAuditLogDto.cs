namespace Media.Application.DTOs;

public record MediaAuditLogDto(Guid Id, string Action, string UserId, string? IpAddress, string? Changes, DateTime Timestamp);

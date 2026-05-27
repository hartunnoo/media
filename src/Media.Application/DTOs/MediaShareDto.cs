namespace Media.Application.DTOs;

public record MediaShareDto(Guid Id, Guid MediaId, string? SharedWithUserId, string PermissionLevel, DateTime? ExpiryDate, DateTime CreatedAt);

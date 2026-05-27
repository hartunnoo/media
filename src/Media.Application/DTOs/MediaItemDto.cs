using Media.Domain.Enums;

namespace Media.Application.DTOs;

public record MediaItemDto(
    Guid Id, string OriginalFileName, string ContentType, long FileSize, string? FileHash,
    MediaStatus Status, string OwnedByUserId, string? OwnedByAppId, Guid? FolderId,
    string? FolderName, bool IsLegalHold, DateTime CreatedAt, string CreatedBy,
    DateTime UpdatedAt, string UpdatedBy, DateTime? DeletedAt);

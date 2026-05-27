using Media.Domain.Enums;

namespace Media.Domain.Entities;

public class MediaItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? FileHash { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public MediaStatus Status { get; set; } = MediaStatus.Queued;
    public string OwnedByUserId { get; set; } = string.Empty;
    public string? OwnedByAppId { get; set; }
    public Guid? FolderId { get; set; }
    public MediaFolder? Folder { get; set; }
    public bool IsLegalHold { get; set; }
    public int RetentionDays { get; set; } = Constants.MediaConstants.DefaultRetentionDays;
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? RestoredAt { get; set; }
    public string? RestoredBy { get; set; }

    public ICollection<MediaVersion> Versions { get; set; } = new List<MediaVersion>();
    public ICollection<MediaItemTag> MediaItemTags { get; set; } = new List<MediaItemTag>();
    public ICollection<MediaShare> Shares { get; set; } = new List<MediaShare>();
    public ICollection<MediaAuditLog> AuditLogs { get; set; } = new List<MediaAuditLog>();
}

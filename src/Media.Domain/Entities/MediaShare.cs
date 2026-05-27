using Media.Domain.Enums;

namespace Media.Domain.Entities;

public class MediaShare
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MediaId { get; set; }
    public MediaItem Media { get; set; } = null!;
    public string? SharedWithUserId { get; set; }
    public string? SharedWithAppId { get; set; }
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.View;
    public DateTime? ExpiryDate { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

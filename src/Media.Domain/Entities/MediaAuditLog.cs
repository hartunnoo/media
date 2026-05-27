using Media.Domain.Enums;

namespace Media.Domain.Entities;

public class MediaAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MediaId { get; set; }
    public MediaItem Media { get; set; } = null!;
    public MediaActionType Action { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Changes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

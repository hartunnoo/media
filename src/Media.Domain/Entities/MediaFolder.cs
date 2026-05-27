namespace Media.Domain.Entities;

public class MediaFolder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public MediaFolder? ParentFolder { get; set; }
    public string OwnedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public ICollection<MediaFolder> Children { get; set; } = new List<MediaFolder>();
    public ICollection<MediaItem> MediaItems { get; set; } = new List<MediaItem>();
}

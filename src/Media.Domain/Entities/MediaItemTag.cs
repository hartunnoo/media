namespace Media.Domain.Entities;

public class MediaItemTag
{
    public Guid MediaItemId { get; set; }
    public MediaItem MediaItem { get; set; } = null!;
    public Guid MediaTagId { get; set; }
    public MediaTag MediaTag { get; set; } = null!;
}

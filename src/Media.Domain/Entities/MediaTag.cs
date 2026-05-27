namespace Media.Domain.Entities;

public class MediaTag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<MediaItemTag> MediaItemTags { get; set; } = new List<MediaItemTag>();
}
